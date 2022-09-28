using EdgeDB.Binary;
using EdgeDB.Binary.Packets;

namespace EdgeDB.Dumps
{
    internal ref struct DumpWriter
    {
        public const long DumpVersion = 1;
        public const int FileFormatLength = 17;

        public static readonly byte[] FileFormat = new byte[]
        {
            0xff, 0xd8, 0x00,0x00, 0xd8,

            // file format "EDGEDB"
            69, 68, 71, 69, 68, 66,

            0x00,

            // file format "DUMP
            68, 85, 77, 80,

            0x00,
        };

        private static byte[] Version
            => BitConverter.GetBytes((long)1).Reverse().ToArray();

        public ReadOnlyMemory<byte> Data => _writer.GetBytes();

        private readonly PacketWriter _writer;
        public DumpWriter()
        {
            _writer = new PacketWriter();

            _writer.Write(FileFormat);
            _writer.Write(Version);
        }

        public void WriteDumpHeader(DumpHeader header)
        {
            _writer.Write('H');
            _writer.Write(header.Hash.ToArray());
            _writer.Write(header.Length);
            _writer.Write(header.Raw);
        }

        public void WriteDumpBlocks(List<DumpBlock> blocks)
        {
            for(int i = 0; i != blocks.Count; i++)
            {
                var block = blocks[i];
                
                _writer.Write('D');
                _writer.Write(block.HashBuffer);
                _writer.Write(block.Length);
                _writer.Write(block.Raw);
            }
        }
    }
}
