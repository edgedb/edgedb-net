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
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal unsafe ref struct PacketWriter
    {
        public long Index
            => _trackedPointer - _basePointer;
        
        internal int Size;
        private Span<byte> _span;
        private byte* _trackedPointer;
        private byte* _basePointer;

        public PacketWriter(int size)
        {
            _span = new byte[size];
            _trackedPointer = (byte*)Unsafe.AsPointer(ref _span.GetPinnableReference());
            _basePointer = (byte*)Unsafe.AsPointer(ref _span.GetPinnableReference());
            Size = size;
        }

        public PacketWriter()
            : this(512)
        {
            
        }

        #region Position & conversion
        public void SeekToIndex(long index)
            => _trackedPointer = _basePointer + index;

        public void Advance(long count)
            => _trackedPointer += count;

        public Span<byte> GetBytes()
            => _span[..(int)Index];
        #endregion

        public delegate void WriteAction(ref PacketWriter writer);
        public void WriteToWithInt32Length(WriteAction action)
        {
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
            var newSize =
                target + Index > 2048
                    ? Size + (int)target + 512
                    : Size > 2048 ? Size + 2048 : (Size << 1) + (int)target;
            
            var index = Index;
            
            Span<byte> newSpan = new byte[newSize];
            _span.CopyTo(newSpan);
            _span = newSpan;
            
            _basePointer = (byte*)Unsafe.AsPointer(ref _span.GetPinnableReference());
            _trackedPointer = _basePointer + index;
            
            Size = newSize;
        }

        private void WriteRaw(void* ptr, uint count)
        {
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
            Write(buffer);
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
    }
}
