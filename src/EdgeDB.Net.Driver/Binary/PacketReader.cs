using EdgeDB.Utils;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EdgeDB.Binary
{
    internal unsafe ref struct PacketReader
    {
        public bool Empty
            => Position >= _limit || Data.IsEmpty;

        internal int Limit
        {
            get => _limit;
            set
            {
                if(value < 0)
                {
                    // reset limit
                    _limit = Data.Length;
                    return;
                }

                if (Position + value > Data.Length)
                    throw new InternalBufferOverflowException($"Setting the buffer limit to {value} would overflow the internal buffer");

                _limit = Position + value;
            }
        }

        internal ReadOnlySpan<byte> Data;
        
        internal int Position;

        private int _limit;

        public PacketReader(ReadOnlySpan<byte> bytes, int position = 0)
        {
            Data = bytes;
            Position = position;
            _limit = Data.Length;
        }

        public PacketReader(scoped in ReadOnlySpan<byte> bytes, int position = 0)
        {
            Data = bytes;
            Position = position;
            _limit = Data.Length;
        }

        private void VerifyInLimits(int sz)
        {
            if (Position + sz > _limit)
                throw new InternalBufferOverflowException("The requested read operation would overflow the buffer");
        }

#region Unmanaged basic reads & endianness correction
        private ref T UnsafeReadAs<T>()
            where T : unmanaged
        {
            VerifyInLimits(sizeof(T));

            ref var ret = ref Unsafe.As<byte, T>(ref Unsafe.AsRef(in Data[Position]));

            BinaryUtils.CorrectEndianness(ref ret);
            Position += sizeof(T);
            return ref ret;
        }

        public ref T ReadStruct<T>()
            where T : unmanaged
            => ref UnsafeReadAs<T>();

        public bool ReadBoolean()
            => ReadByte() > 0;

        public byte ReadByte()
            => UnsafeReadAs<byte>();

        public char ReadChar()
            => UnsafeReadAs<char>();

        public double ReadDouble()
            => UnsafeReadAs<double>();

        public float ReadSingle()
            => UnsafeReadAs<float>();

        public ulong ReadUInt64()
            => UnsafeReadAs<ulong>();

        public long ReadInt64()
            => UnsafeReadAs<long>();

        public uint ReadUInt32()
            => UnsafeReadAs<uint>();

        public int ReadInt32()
            => UnsafeReadAs<int>();

        public ushort ReadUInt16()
            => UnsafeReadAs<ushort>();

        public short ReadInt16()
            => UnsafeReadAs<short>();
#endregion

        public void Skip(int count)
        {
            VerifyInLimits(count);
            Position += count;
        }

        public byte[] ConsumeByteArray()
        {
            var data =  Data[Position.._limit].ToArray();
            Position = _limit;
            return data;
        }

        public string ConsumeString()
        {
            var str = Encoding.UTF8.GetString(Data[Position.._limit]);
            Position = _limit;
            return str;
        }

        public Guid ReadGuid()
        {
            VerifyInLimits(sizeof(Guid));

            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();
            
            var buffer = Data[Position..(Position + 8)];
            var guid = new Guid(a, b, c, buffer.ToArray());
            Position += 8;
            return guid;
        }
        
        public KeyValue[] ReadKeyValues()
        {
            VerifyInLimits(sizeof(ushort));
            var count = ReadUInt16();
            var arr = new KeyValue[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadKeyValue();
            }
            
            return arr;
        }

        public KeyValue ReadKeyValue()
        {
            VerifyInLimits(sizeof(ushort));

            var code = ReadUInt16();
            var value = ReadByteArray();

            return new KeyValue(code, value);
        }

        public string ReadString()
        {
            VerifyInLimits(sizeof(int));
            var strLength = (int)ReadUInt32();
            VerifyInLimits(strLength);
            var str = Encoding.UTF8.GetString(Data[Position..(strLength + Position)]);
            Position += strLength;
            return str;
        }

        public Annotation[] ReadAnnotations()
        {
            VerifyInLimits(sizeof(ushort));
            var count = ReadUInt16();

            Annotation[] arr = new Annotation[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadAnnotation();
            }

            return arr;
        }

        public Annotation ReadAnnotation()
        {
            var name = ReadString();
            var value = ReadString();

            return new Annotation(name, value);
        }

        public byte[] ReadByteArray()
        {
            VerifyInLimits(sizeof(int));
            var length = (int)ReadUInt32();
            VerifyInLimits(length);
            var buffer = Data[Position..(Position + length)];
            Position += length;
            return buffer.ToArray();
        }

        public void ReadBytes(int length, out ReadOnlySpan<byte> buff)
        {
            VerifyInLimits(length);
            buff = Data[Position..(Position + length)];
            Position += length;
        }
    }
}
