using EdgeDB.Models;
using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class PacketWriter : BinaryWriter
    {
        public PacketWriter()
            : base(new MemoryStream())
        {

        }

        public PacketWriter(Stream stream)
            : base(stream, Encoding.UTF8, true)
        {

        }

        public void Write(IEnumerable<Header>? headers)
        {
            // write length
            Write((ushort)(headers?.Count() ?? 0));

            if(headers is not null)
            {
                foreach (var header in headers)
                {
                    Write(header.Code);
                    WriteArray(header.Value);
                }
            }
        }

        public void Write(PacketWriter value, int offset = 0)
        {
            // block copy data to other stream
            value.BaseStream.Position = offset;
            value.BaseStream.CopyTo(base.BaseStream);
        }

        public void Write(Header header)
        {
            Write(header.Code);
            Write(header.Value);
        }

        public void Write(Guid value)
        {
            var bytes = HexConverter.FromHex(value.ToString().Replace("-", ""));

            Write(bytes);
        }

        public override void Write(string value)
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
            var a = BitConverter.GetBytes(value).Reverse().ToArray(); 
            Write(a);
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

        public void WriteArrayWithoutLength(byte[] buffer)
            => base.Write(buffer);
    }
}
