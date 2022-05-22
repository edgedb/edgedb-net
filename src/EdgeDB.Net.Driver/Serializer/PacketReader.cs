using EdgeDB.Models;
using EdgeDB.Utils;
using System.Text;

namespace EdgeDB
{
    internal class PacketReader : BinaryReader
    {
        public PacketReader(Stream input) : base(input, Encoding.UTF8, true)
        {
        }

        public PacketReader(byte[] buff)
            : this(new MemoryStream(buff))
        {

        }

        public byte[] ConsumeByteArray() // should be smarter than this, ex: empty instead of taking a max of 1024 bytes
        {
            byte[] buff = new byte[BaseStream.Length - BaseStream.Position];
            base.BaseStream.Read(buff, 0, buff.Length);
            return buff;
        }

        public string ConsumeString()
        {
            using var streamReader = new StreamReader(base.BaseStream, Encoding.UTF8, leaveOpen: true);
            return streamReader.ReadToEnd();
        }

        public Guid ReadGuid()
        {
            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();

            Span<byte> buffer = stackalloc byte[8];
            if (base.Read(buffer) != 8)
                throw new EndOfStreamException();

            return new Guid(a, b, c, buffer.ToArray());
        }

        public override double ReadDouble()
        {
            Span<byte> buffer = stackalloc byte[8];
            if (base.Read(buffer) != 8)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToDouble(buffer);
        }

        public override float ReadSingle()
        {
            Span<byte> buffer = stackalloc byte[4];
            if (base.Read(buffer) != 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToSingle(buffer);
        }

        public override ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            if (base.Read(buffer) != 8)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt64(buffer);
        }

        public override long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            if (base.Read(buffer) != 8)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt64(buffer);
        }

        public override uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            if (base.Read(buffer) != 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt32(buffer);
        }

        public override int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            if (base.Read(buffer) != 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt32(buffer);
        }

        public override ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            if (base.Read(buffer) != 2)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt16(buffer);
        }

        public override short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            if (base.Read(buffer) != 2)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt16(buffer);
        }

        public override string ReadString()
        {
            var lenth = ReadUInt32();

            Span<byte> buffer = stackalloc byte[(int)lenth];

            if (base.Read(buffer) != lenth)
                throw new EndOfStreamException();

            return Encoding.UTF8.GetString(buffer);
        }

        public Header[] ReadHeaders()
        {
            var length = ReadUInt16();

            Header[] arr = new Header[length];

            for (ushort i = 0; i != length; i++)
            {
                arr[i] = ReadHeader();
            }

            return arr;
        }

        public Header ReadHeader()
        {
            var code = ReadUInt16();

            var arr = ReadByteArray();

            return new Header(code, arr);
        }

        public byte[] ReadByteArray()
        {
            var length = ReadUInt32();

            Span<byte> buffer = stackalloc byte[(int)length];

            if (base.Read(buffer) != length)
                throw new EndOfStreamException();

            return buffer.ToArray();
        }
    }
}
