using EdgeDB.Binary;
using EdgeDB.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal unsafe ref struct PacketWriter
    {
        public readonly long Index
            => _trackedPointer - _basePointer;

        public readonly bool CanWrite
            => _canWrite;
        
        internal int Size;
        
        private readonly bool _isDynamic;
        
        private Memory<byte> _rawBuffer;
        private MemoryHandle _handle;
        private IMemoryOwner<byte>? _memoryOwner;

        private byte* _trackedPointer;
        private byte* _basePointer;

        private bool _canWrite;


        public PacketWriter(int size, bool isDynamic = false)
        {
            // use shared memory if our size is fixed
            if (size > MemoryPool<byte>.Shared.MaxBufferSize || isDynamic)
            {
                _rawBuffer = new Memory<byte>(new byte[size]);
                _memoryOwner = null;
            }
            else
            {
                _memoryOwner = MemoryPool<byte>.Shared.Rent(size);
                _rawBuffer = _memoryOwner.Memory;
            }

            _canWrite = true;
            _handle = _rawBuffer.Pin();
            _trackedPointer = (byte*)_handle.Pointer;
            _basePointer = (byte*)_handle.Pointer;
            Size = size;
            _isDynamic = isDynamic;
        }

        public PacketWriter()
            : this(512, true)
        {
            
        }

        #region Position & conversion
        public void SeekToIndex(long index)
            => _trackedPointer = _basePointer + index;

        public void Advance(long count)
            => _trackedPointer += count;

        public ReadOnlyMemory<byte> GetBytes()
        {
            var buffer = _rawBuffer[..(_isDynamic ? (int)Index : Size)];
            Free();
            return buffer;
        }

        public ReadOnlyMemory<byte> GetRetainedBytes()
            => _rawBuffer[..(_isDynamic ? (int)Index : Size)];

        #endregion

        private void ThrowIfCantWrite()
        {
            if (!_canWrite)
            {
                throw new EdgeDBException("Cannot write/modify the underlying free'd buffer");
            }
        }

        public delegate void WriteAction(ref PacketWriter writer);
        public void WriteToWithInt32Length(WriteAction action)
        {
            ThrowIfCantWrite();

            var currentIndex = Index;
            Advance(sizeof(int));
            action(ref this);
            var lastIndex = Index;
            SeekToIndex(currentIndex);
            Write((int)(lastIndex - currentIndex - sizeof(int)));
            SeekToIndex(lastIndex);
        }

        #region Very raw memory writing methods for unmanaged
        private void Resize(uint target)
        {
            ThrowIfCantWrite();

            if (!_isDynamic)
                throw new IndexOutOfRangeException("Cannot resize a non-dynamic packet writer");

            var newSize =
                target + Index > 2048
                    ? Size + (int)target + 512
                    : Size > 2048 ? Size + 2048 : (Size << 1) + (int)target;
            
            var index = Index;

            var newBuffer = new Memory<byte>(new byte[newSize]);
            _rawBuffer.CopyTo(newBuffer);
            _rawBuffer = newBuffer;
            _handle.Dispose();
            _handle = newBuffer.Pin();
            _basePointer = (byte*)_handle.Pointer;
            _trackedPointer = _basePointer + index;

            Size = newSize;
        }

        private void WriteRaw(void* ptr, uint count)
        {
            ThrowIfCantWrite();
            
            if (Index + count > Size)
                Resize(count);

            Unsafe.CopyBlock(_trackedPointer, ptr, count);
            _trackedPointer += count;
        }

        public void Write(ref PacketWriter value)
        {
            WriteRaw(value._basePointer, (uint)value.Index);
        }

        private void UnsafeWrite<T>(ref T value)
            where T : unmanaged
        {
            ThrowIfCantWrite();
            
            var size = sizeof(T);
            if (Index + size > Size)
                Resize((uint)size);

            BinaryUtils.CorrectEndianness(ref value);
            Unsafe.WriteUnaligned(_trackedPointer, value);
            _trackedPointer += sizeof(T);
        }

        public void Write<T>(T value)
            where T : unmanaged
            => UnsafeWrite<T>(ref value);

        public void Write(double value)
            => UnsafeWrite(ref value);

        public void Write(float value)
            => UnsafeWrite(ref value);

        public void Write(ulong value)
            => UnsafeWrite(ref value);

        public void Write(long value)
            => UnsafeWrite(ref value);

        public void Write(uint value)
            => UnsafeWrite(ref value);

        public void Write(int value)
            => UnsafeWrite(ref value);

        public void Write(short value)
            => UnsafeWrite(ref value);

        public void Write(ushort value)
            => UnsafeWrite(ref value);

        public void Write(byte value)
            => UnsafeWrite(ref value);

        public void Write(sbyte value)
            => UnsafeWrite(ref value);

        public void Write(char value)
            => UnsafeWrite(ref value);

        public void Write(bool value)
            => UnsafeWrite(ref value);
        #endregion
        
        public void WriteArray(byte[] buffer)
        {
            Write((uint)buffer.Length);

            if(buffer.Length > 0)
                Write(buffer);
        }

        public void WriteArray(ReadOnlyMemory<byte> buffer)
        {
            Write((uint)buffer.Length);

            if (buffer.Length > 0)
                Write(ref buffer);
        }
        
        public void WriteArrayWithoutLength(byte[] buffer)
            => Write(buffer);

        public void Write(IEnumerable<Annotation>? headers)
        {
            // write length
            Write((ushort)(headers?.Count() ?? 0));

            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    Write(header.Name);
                    Write(header.Value);
                }
            }
        }
        
        public void Write(KeyValue header)
        {
            Write(header.Code);
            Write(header.Value);
        }

        public void Write(Guid value)
        {
            var bytes = HexConverter.FromHex(value.ToString().Replace("-", ""));
            Write(bytes);
        }

        public void Write(byte[] array)
        {
            fixed (byte* ptr = array)
            {
                WriteRaw(ptr, (uint)array.Length);
            }
        }

        public void Write(ref ReadOnlyMemory<byte> memory)
        {
            using var pin = memory.Pin();

            WriteRaw(pin.Pointer, (uint)memory.Length);

            pin.Dispose();
        }

        public void Write(string value)
        {
            if (value is null)
                Write((uint)0);
            else
            {
                var buffer = Encoding.UTF8.GetBytes(value);
                Write((uint)buffer.Length);
                Write(buffer);
            }
        }

        public void Reset()
        {
            _rawBuffer.Span.Clear();
            _trackedPointer = _basePointer;
        }

        /// <summary>
        ///     Unpins the raw memory, allowing the buffer to
        ///     be managed by the GC.
        /// </summary>
        /// <remarks>
        ///     After this operation, the writer can no
        ///     longer be written to.
        /// </remarks>
        public void Free()
        {
            _handle.Dispose();
            
            _canWrite = false;
        }

        /// <summary>
        ///     Disposes the packet writer, freeing the raw memory to be
        ///     garbage collected.
        /// </summary>
        /// <remarks>
        ///     The buffer returned from <see cref="GetBytes"/> is not safe
        ///     to use after this method is called.
        /// </remarks>
        public void Dispose()
        {
            Free();
            _memoryOwner?.Dispose();

            _trackedPointer = null;
            _basePointer = null;
            _memoryOwner = null;

            _canWrite = false;
        }
    }
}
