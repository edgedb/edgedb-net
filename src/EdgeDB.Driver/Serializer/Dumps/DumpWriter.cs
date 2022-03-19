using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Dumps
{
    internal class DumpWriter
    {
        private readonly Stream _stream;
        private readonly PacketWriter _writer;
        public DumpWriter(Stream output)
        {
            _stream = output;
            _writer = new PacketWriter(_stream);

            var bytes = new byte[]
            {
                0xff, 0xd8, 0x00,0x00, 0xd8,

                // file format "EDGEDB"
                69, 68, 71, 69, 68, 66,

                0x00,

                // file format "DUMP
                68, 85, 77, 80,

                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1
            };

            _writer.Write(bytes);
        }

        public void WriteDumpHeader(DumpHeader header)
        {
            _writer.Write('H');
            _writer.Write(header.Hash.ToArray());
            _writer.Write(header.Length);
            _writer.Write(header.Raw);

            _writer.Flush();
        }

        public void WriteDumpBlock(DumpBlock block)
        {
            _writer.Write('D');
            _writer.Write(block.Hash.ToArray());
            _writer.Write(block.Length);
            _writer.Write(block.Raw);

            _writer.Flush();
        }
    }
}
