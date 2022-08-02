using EdgeDB.Binary;
using EdgeDB.Models;
using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal unsafe class PacketWriter : BinaryWriter
    {
        public long Length
            => base.OutStream.Length;

        public PacketWriter()
            : base(new MemoryStream())
        {

        }

        public PacketWriter(Stream stream)
            : base(stream, Encoding.UTF8, true)
        {

        }

        public byte[] GetBytes()
        {
            if (BaseStream is not MemoryStream ms)
                throw new InvalidOperationException();

            ms.Position = 0;

            return ms.ToArray();
        }

        public void Write(IEnumerable<KeyValue>? headers)
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

        public void Write(PacketWriter value, int offset = 0)
        {
            value.BaseStream.Position = offset;
            value.BaseStream.CopyTo(base.BaseStream);
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
            var span = new Span<byte>((byte*)&value, sizeof(double));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(float value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(float));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(uint value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(uint));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(int value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(int));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(ulong value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(ulong));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(long value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(long));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(short value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(short));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
        }

        public override void Write(ushort value)
        {
            var span = new Span<byte>((byte*)&value, sizeof(ushort));
            if (BitConverter.IsLittleEndian)
                span.Reverse();
            Write(span);
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
