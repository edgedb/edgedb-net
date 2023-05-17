using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    internal sealed class PacketContract : IDisposable
    {
        public unsafe readonly struct Handle
        {
            public PacketContract Value
                => Unsafe.As<IntPtr, PacketContract>(ref *(IntPtr*)_handle.AddrOfPinnedObject());

            private readonly GCHandle _handle;

            public Handle(PacketContract contract)
            {
                _handle = contract._thisPin;
            }
        }

        internal unsafe struct BufferContract : IDisposable
        {
            public readonly ReservedBuffer* Buffer;

            private void* _pointer;

            private int _disposed;

            public BufferContract(int start, int length)
            {
                Buffer = (ReservedBuffer*)NativeMemory.Alloc((nuint)sizeof(ReservedBuffer));

                *Buffer = new ReservedBuffer((BufferContract*)Unsafe.AsPointer(ref this), start, length);
            }

            public BufferContract(int start, int length, PacketContract contract)
                : this(start, length)
            {
                RegisterContract(contract);
            }

            public PacketReader CreateReader()
            {
                if (_pointer == null)
                {
                    throw new NullReferenceException();
                }

                return new PacketReader(Unsafe.AsRef<PacketContract>(_pointer).ContractHandle);
            }

            public void RegisterContract(PacketContract contract)
            {
                _pointer = (void*)contract._thisPin.AddrOfPinnedObject();
            }

            public void Dispose()
            {
                if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                {
                    NativeMemory.Free(Buffer);
                }
            }
        }

        public Handle ContractHandle
            => new(this);

        public bool IsEmpty
            => _position == _length;

        public int Length
            => _length;

        public int Position
            => _position;

        [MemberNotNullWhen(
            false,
            nameof(_chunk), nameof(_allocs), nameof(_reservedBuffers),
            nameof(_subContracts), nameof(_source), nameof(_duplexer)
        )]
        private unsafe bool IsCompletedContract
            => _pointer != null;

        [MemberNotNullWhen(true, nameof(_duplexer))]
        private bool ShouldSignalComplete
            => IsEmpty && _duplexer != null && !_signaledComplete;

        private readonly Stream? _source;

        
        private readonly byte[]? _chunk;
        private int _chunkPosition;
        private int _readTrack;

        private readonly int _length;
        private int _position;

        private bool _firstRead = true;
        private readonly List<nuint> _allocs;

        private readonly Queue<BufferContract>? _reservedBuffers;

        private readonly List<PacketContract>? _subContracts;

        private readonly object _lock;

        // this field is only set if this contract was constructed as a complete contract.
        private readonly unsafe byte* _pointer;

        private readonly GCHandle _thisPin;

        private readonly bool _isSubContract;

        private readonly IBinaryDuplexer? _duplexer;
        private bool _signaledComplete;

        private PacketContract(IBinaryDuplexer? duplexer)
        {
            _duplexer = duplexer;
            _allocs = new();
            _lock = new object();
            _thisPin = GCHandle.Alloc(this, GCHandleType.Pinned);
        }

        internal unsafe PacketContract(IBinaryDuplexer duplexer, Stream source, PacketHeader* header, int chunkSize)
            : this(duplexer)
        {
            _length = header->Length;
            _source = source;
            _chunk = ArrayPool<byte>.Shared.Rent(chunkSize);
            _reservedBuffers = new();
            _subContracts = new();
        }

        private PacketContract(IBinaryDuplexer duplexer, PacketContract parent, int length, int position = 0)
            : this(duplexer)
        {
            _isSubContract = true;
            _length = length;
            _position = position;
            _source = parent._source;
            _chunk = parent._chunk;
            _reservedBuffers = parent._reservedBuffers;
            _subContracts = new();
        }

        private unsafe PacketContract(byte* pointer, int length)
            : this((IBinaryDuplexer?)null)
        {
            _length = length;
            _pointer = pointer;
        }

        internal unsafe PacketContract(Span<byte> data)
            : this((byte*)Unsafe.AsPointer(ref data.GetPinnableReference()), data.Length)
        {
        }

        public unsafe ReservedBuffer* ReserveRead(int count)
        {
            if(IsCompletedContract)
            {
                throw new InvalidOperationException("Cannot reserve buffers from a completed contract");
            }

            var subContract = new PacketContract(_duplexer, this, count);

            var bufferContract = new BufferContract(_position, count, subContract);

            _subContracts.Add(subContract);

            _reservedBuffers.Enqueue(bufferContract);

            return bufferContract.Buffer;
        }

        public unsafe void Skip(int count)
        {
            if (!IsCompletedContract)
            {
                CheckAndCompleteBufferContracts(count);
            }

            var ptr = (byte*)NativeMemory.Alloc((nuint)count);
            ReadIntoPointer(ptr, count);
            NativeMemory.Free(ptr);

            CheckComplete();
        } 

        public unsafe Span<byte> RequestBuffer(int sz, bool copy = false)
        {
            if (_position + sz > _length)
            {
                throw new InvalidOperationException($"Reading {sz} bytes would exceed the packet size");
            }

            if(sz <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sz), "Requested buffer size must be greater than zero");
            }

            if (IsCompletedContract)
            {
                return new Span<byte>(_pointer + _position, sz);
            }

            CheckAndCompleteBufferContracts(sz);

            Span<byte> span;

            ref var elementReference = ref _chunk[_chunkPosition];

            if (sz + _chunkPosition < _chunk.Length)
            {
                if (copy)
                {
                    var ptr = TrackedAlloc(sz);
                    ReadIntoPointer(ptr, sz);
                    span = new Span<byte>(ptr, sz);
                }
                else
                {
                    span = new Span<byte>(Unsafe.AsPointer(ref elementReference), sz);
                }
            }
            else
            {
                var ptr = TrackedAlloc(sz);

                ReadIntoPointer(ptr, sz);

                span = new Span<byte>(ptr, sz);

                // refil the chunk
                ReadChunk();
            }

            if (ShouldSignalComplete && !copy)
            {
                // span can point to memory which will be
                // free'd on the complete call, make a copy
                var copied = new byte[span.Length].AsSpan();
                span.CopyTo(copied);
                span = copied;
            } 

            CheckComplete();

            return span;
        }

        private unsafe void ReadIntoPointer(byte* ptr, int size)
        {
            if (_position + size > _length)
            {
                throw new InvalidOperationException($"Reading {size} bytes would exceed the packet size");
            }

            if (IsCompletedContract)
            {
                Unsafe.CopyBlockUnaligned(ref *ptr, ref *_pointer, (uint)size);
                _position += size;
                return;
            }

            // check chunk for remaing data
            var chunkReadLength = _chunk.Length - _chunkPosition - 1;

            var ptrOffset = 0;

            if(chunkReadLength > 0)
            {
                Unsafe.CopyBlockUnaligned(ref *ptr, ref _chunk[_chunkPosition], (uint)chunkReadLength);
                ptrOffset += chunkReadLength;
            }

            // read size from the stream
            var span = new Span<byte>(ptr + ptrOffset, size);

            ReadExact(span);

            _position += size;

            CheckComplete();
        }

        public ref T Request<T>()
            where T : unmanaged
        {
            lock (_lock)
            {
                return ref RequestInternal<T>();
            }
        }

        private unsafe ref T RequestInternal<T>()
            where T : unmanaged
        {
            if(IsCompletedContract)
            {
                if(sizeof(T) + _position > _length)
                {
                    throw new InvalidOperationException($"Reading {sizeof(T)} bytes would exceed the packet size");
                }

                var ptr = (T*)(_pointer + _position);
                _position += sizeof(T);

                CheckComplete();

                return ref *ptr;
            }

            if (_firstRead)
            {
                ReadChunk();
                _firstRead = false;
            }

            CheckAndCompleteBufferContracts(sizeof(T));

            var tSize = sizeof(T);

            var target = tSize + _chunkPosition;

            // buffer isn't full, but is missing data,
            while (RequiresRead(target) && target < _chunk.Length)
            {
                ReadChunk();
            }

            try
            {
                if (tSize + _chunkPosition >= _chunk.Length)
                {
                    return ref Unsafe.AsRef<T>(PreformChunkExchange<T>());
                }
                else
                {
                    return ref PreformRefChange<T>();
                }
            }
            finally
            {
                CheckComplete();
            }

        }

        private unsafe T* PreformChunkExchange<T>()
            where T : unmanaged
        {
            if(IsCompletedContract)
            {
                Debug.Fail("Attempted to chunk exchange buffers when the contract was complete");

                var tPointer = (T*)_pointer;
                _position += sizeof(T);
                return tPointer;
            }

            var ptr = (byte*)NativeMemory.Alloc((nuint)sizeof(T));

            _allocs.Add((nuint)ptr);

            var count = _chunk.Length - _chunkPosition;
            var remaining = sizeof(T) - count;

            // copy remaining bytes into the pointer
            Unsafe.CopyBlockUnaligned(ref *ptr, ref _chunk[_chunkPosition], (uint)count);

            // preform read
            ReadChunk();

            // copy the remaing data into the ptr
            Unsafe.CopyBlockUnaligned(ref *(ptr + count), ref _chunk[0], (uint)remaining);

            _chunkPosition += remaining;
            _position += remaining;

            return (T*)ptr;
        }

        private void ReadChunk()
        {
            if (IsCompletedContract)
                return;

            // reset the read track if the entire chunk has been read
            if(_readTrack == _chunk.Length - 1)
            {
                _readTrack = 0;
            }

            var dist = _chunk.Length - _readTrack;
            var count = _source.Read(_chunk, _readTrack, dist);

            if(count == 0)
            {
                OnDisconnect();
                return;
            }

            _readTrack += count;
        }

        private unsafe ref T PreformRefChange<T>()
            where T : unmanaged
        {
            if(IsCompletedContract)
            {
                Debug.Fail("Attempted to ref exchange when the contract was complete");

                var ptr = (T*)(_pointer + _position);
                _position += sizeof(T);
                return ref *ptr;
            }

            ref var pos = ref _chunk[_chunkPosition];

            _chunkPosition += sizeof(T);
            _position += sizeof(T);

            return ref Unsafe.As<byte, T>(ref pos);
        }

        private bool RequiresRead(int sz)
        {
            return _chunkPosition + sz > _readTrack;
        }

        private unsafe byte* TrackedAlloc(int size)
        {
            var ptr = (byte*)NativeMemory.Alloc((nuint)size);
            _allocs.Add((nuint)ptr);
            return ptr;
        }

        private void ReadExact(Span<byte> buffer)
        {
            if(IsCompletedContract)
            {
                Debug.Fail("Attempted to call stream read from a completed contract");

                if(buffer.Length + _position > _length)
                {
                    throw new InvalidOperationException($"Reading {buffer.Length} bytes would exceed the packet size");
                }

                unsafe
                {
                    Unsafe.CopyBlockUnaligned(ref buffer.GetPinnableReference(), ref *(_pointer + _position), (uint)buffer.Length);
                }

                return;
            }

#if NET7_0_OR_GREATER
            _source.ReadExactly(buffer);
#else
            var targetLength = buffer.Length;

            int numRead = 0;

            while (numRead < targetLength)
            {
                var buff = numRead == 0
                    ? buffer
                    : buffer[numRead..];

                var read = _source.Read(buff[numRead..]);

                if (read == 0) // disconnected
                {
                    OnDisconnect();
                    return;
                }

                numRead += read;
            }
#endif
        }

        private unsafe void CheckAndCompleteBufferContracts(int size)
        {
            if(IsCompletedContract)
            {
                Debug.Fail("Completed contract attempted to check for reserved buffers");
                return;
            }

            if(_reservedBuffers.TryPeek(out var contract))
            {
                // is the contract within our range
                if(_position == contract.Buffer->PacketPosition)
                {
                    // complete the contract
                    var ptr = TrackedAlloc(contract.Buffer->Length);

                    ReadIntoPointer(ptr, contract.Buffer->Length);

                    contract.RegisterContract(new PacketContract(ptr, contract.Buffer->Length));
                    _reservedBuffers.Dequeue();
                }
                else if(_position > contract.Buffer->PacketPosition && _position + size < contract.Buffer->PacketPosition)
                {
                    // fail because the read operation spans over multiple sections. this shouldn't ever occor
                    throw new InvalidOperationException("The requested read spans over multiple reserved buffer sections");
                }
            }
        }

        private void OnDisconnect()
        {
            if(_duplexer != null)
            {
                _duplexer.OnContractDisconnected(this);
            }

            throw new EndOfStreamException();
        }

        private void CheckComplete()
        {
            if (ShouldSignalComplete)
            {
                _duplexer.OnContractComplete(this);
                _signaledComplete = true;
            }
        }

        public void Dispose()
        {
            if(!IsCompletedContract && !_isSubContract)
            {
                ArrayPool<byte>.Shared.Return(_chunk);
            }

            unsafe
            {
                foreach (var alloc in _allocs)
                {
                    NativeMemory.Free((void*)alloc);
                }
            }
        }
    }
}
