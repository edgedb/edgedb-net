using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class PacketWriter : BinaryWriter
    {
        public PacketWriter()
            : base(new MemoryStream())
        {

        }

        public PacketWriter(Stream stream)
            : base(stream, Encoding.UTF8, true)
        {

        }

        public void Write(PacketWriter value)
        {
            // block copy data to other stream
            value.BaseStream.CopyTo(base.BaseStream);
        }

        public void Write(Header header)
        {
            Write(header.Code);
            Write(header.Value);
        }

        public void Write(Guid value)
        {
            Write(value.ToByteArray());
        }

        public override void Write(string value)
        {
            Write((uint)value.Length);

            Write(Encoding.UTF8.GetBytes(value));
        }

        public override void Write(double value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(float value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(uint value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(int value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(ulong value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(long value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(short value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(ushort value)
        {
            Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public void WriteArray(byte[] buffer)
        {
            Write((uint)buffer.Length);
            base.Write(buffer);
        }
    }
}
