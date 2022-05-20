using EdgeDB.Models;

namespace EdgeDB.Dumps
{
    internal class DumpWriter : IDisposable
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

        private readonly Stream _stream;
        private readonly PacketWriter _writer;
        public DumpWriter(Stream output)
        {
            _stream = output;
            _writer = new PacketWriter(_stream);

            _writer.Write(FileFormat);
            _writer.Write(Version);
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
            _writer.Write(block.HashBuffer);
            _writer.Write(block.Length);
            _writer.Write(block.Raw);

            _writer.Flush();
        }

        public void Dispose() => _writer.Dispose();
    }
}
