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

        public byte[] ConsumeByteArray()
        {
            byte[] buff = new byte[1024];

            var l = base.BaseStream.Read(buff, 0, buff.Length);

            return buff.Take(l).ToArray();
        }

        public string ConsumeString()
        {
            using var streamReader = new StreamReader(base.BaseStream, Encoding.UTF8, leaveOpen: true);
            return streamReader.ReadToEnd();
        }

        public Guid ReadGuid()
        {
            var bytes = ReadBytes(16);

            return Guid.Parse(HexConverter.ToHex(bytes));
        }

        public override double ReadDouble()
        {
            var bytes = ReadBytes(8);

            return BitConverter.ToDouble(bytes.Reverse().ToArray());
        }

        public override float ReadSingle()
        {
            var bytes = ReadBytes(4);

            return BitConverter.ToSingle(bytes.Reverse().ToArray());
        }

        public override ulong ReadUInt64()
        {
            var bytes = ReadBytes(8);

            return BitConverter.ToUInt64(bytes.Reverse().ToArray());
        }

        public override long ReadInt64()
        {
            var bytes = ReadBytes(8);

            return BitConverter.ToInt64(bytes.Reverse().ToArray());
        }

        public override uint ReadUInt32()
        {
            var bytes = ReadBytes(4);

            return BitConverter.ToUInt32(bytes.Reverse().ToArray());
        }

        public override int ReadInt32()
        {
            var bytes = ReadBytes(4);

            return BitConverter.ToInt32(bytes.Reverse().ToArray());
        }

        public override ushort ReadUInt16()
        {
            var bytes = ReadBytes(2);

            return BitConverter.ToUInt16(bytes.Reverse().ToArray());
        }

        public override short ReadInt16()
        {
            var bytes = ReadBytes(2);

            return BitConverter.ToInt16(bytes.Reverse().ToArray());
        }

        public override string ReadString()
        {
            var lenth = ReadUInt32();

            var l = ReadBytes((int)lenth);

            return Encoding.UTF8.GetString(l);
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

            return new Header
            {
                Code = code,
                Value = arr
            };
        }

        public byte[] ReadByteArray()
        {
            var length = ReadUInt32();
            byte[] data = new byte[length];

            for (int i = 0; i != length; i++)
            {
                data[i] = ReadByte();
            }

            return data;
        }
    }
}
