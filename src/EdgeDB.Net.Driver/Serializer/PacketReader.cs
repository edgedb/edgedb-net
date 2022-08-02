using EdgeDB.Binary;
using EdgeDB.Models;
using EdgeDB.Utils;
using System.Buffers.Binary;
using System.Text;

namespace EdgeDB
{
    internal ref struct PacketReader
    {
        public bool Empty
            => _span.IsEmpty;

        private Span<byte> _span;

        public PacketReader(Span<byte> bytes)
        {
            _span = bytes;
        }

        public PacketReader(ref Span<byte> bytes)
        {
            _span = bytes;
        }

        public void Skip(int count)
        {
            _span = _span[count..];
        }

        public byte[] ConsumeByteArray()
        {
            return _span.ToArray();
        }

        public string ConsumeString()
        {
            return Encoding.UTF8.GetString(_span);
        }

        public Guid ReadGuid()
        {
            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();

            Span<byte> buffer = _span[0..8];
            _span = _span[8..];
            return new Guid(a, b, c, buffer.ToArray());
        }

        public bool ReadBoolean()
            => ReadByte() > 0;

        public byte ReadByte()
        {
            var val = _span[0];
            _span = _span[1..];
            return val;
        }

        public char ReadChar()
            => (char)ReadByte();

        public double ReadDouble()
        {
            var value = BinaryPrimitives.ReadDoubleBigEndian(_span);
            _span = _span[sizeof(double)..];
            return value;
        }

        public KeyValue[] ReadKeyValues()
        {
            var length = ReadUInt16();

            KeyValue[] arr = new KeyValue[length];

            for (ushort i = 0; i != length; i++)
            {
                arr[i] = ReadKeyValue();
            }

            return arr;
        }

        public KeyValue ReadKeyValue()
        {
            var code = ReadUInt16();
            var value = ReadByteArray();

            return new KeyValue(code, value);
        }

        public float ReadSingle()
        {
            var value = BinaryPrimitives.ReadSingleBigEndian(_span);
            _span = _span[sizeof(float)..];
            return value;
        }

        public ulong ReadUInt64()
        {
            var value = BinaryPrimitives.ReadUInt64BigEndian(_span);
            _span = _span[sizeof(ulong)..];
            return value;
        }

        public long ReadInt64()
        {
            var value = BinaryPrimitives.ReadInt64BigEndian(_span);
            _span = _span[sizeof(long)..];
            return value;
        }

        public uint ReadUInt32()
        {
            var value = BinaryPrimitives.ReadUInt32BigEndian(_span);
            _span = _span[sizeof(uint)..];
            return value;
        }

        public int ReadInt32()
        {
            var value = BinaryPrimitives.ReadInt32BigEndian(_span);
            _span = _span[sizeof(int)..];
            return value;
        }

        public ushort ReadUInt16()
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(_span);
            _span = _span[sizeof(ushort)..];
            return value;
        }

        public short ReadInt16()
        {
            var value = BinaryPrimitives.ReadInt16BigEndian(_span);
            _span = _span[sizeof(short)..];
            return value;
        }

        public string ReadString()
        {
            var length = (int)ReadUInt32();
            var str = Encoding.UTF8.GetString(_span[0..length]);
            _span = _span[length..];
            return str;
        }

        public Annotation[] ReadAnnotations()
        {
            var length = ReadUInt16();

            Annotation[] arr = new Annotation[length];

            for (ushort i = 0; i != length; i++)
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
            var length = (int)ReadUInt32();
            var buffer = _span[0..length];
            _span = _span[length..];
            return buffer.ToArray();
        }

        public void ReadBytes(int length, out Span<byte> buff)
        {
            buff = _span[0..length];
            _span = _span[length..];
        }

        public void Dispose()
        {
            _span.Clear();
        }
    }
}
