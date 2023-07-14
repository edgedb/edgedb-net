using EdgeDB.Binary.Protocol.V1._0.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.DumpRestore.V1._0
{
    internal sealed class DumpState
    {
        private readonly Stream _stream;

        public DumpState(Stream stream)
        {
            _stream = stream;
        }

        public ValueTask WriteHeaderAsync(in DumpHeader header)
        {
            var writer = new PacketWriter();
            writer.Write('H');
            writer.Write(header.Hash.ToArray());
            writer.Write(header.Length);
            writer.Write(header.Raw);
            return _stream.WriteAsync(writer.GetBytes());
        }

        public ValueTask WriteBlockAsync(in DumpBlock block)
        {
            var writer = new PacketWriter();
            writer.Write('D');
            writer.Write(block.HashBuffer);
            writer.Write(block.Length);
            writer.Write(block.Raw);
            return _stream.WriteAsync(writer.GetBytes());
        }
    }
}
