using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using System.Security.Cryptography;

namespace EdgeDB.Dumps
{
    internal sealed class DumpReader
    {
        public static (Restore Restore, IEnumerable<RestoreBlock> Blocks) ReadDatabaseDump(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException($"Cannot read from {nameof(stream)}");

            // read file format
            var formatLength = DumpWriter.FileFormatLength;
            var format = new byte[formatLength];
            ThrowIfEndOfStream(stream.Read(format) == formatLength);

            if (!format.SequenceEqual(DumpWriter.FileFormat))
                throw new FormatException("Format of stream does not match the edgedb dump format");

            Restore? restore = null;
            List<RestoreBlock> blocks = new();

            Span<byte> buff;
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                if (ms.TryGetBuffer(out var arr))
                    buff = arr;
                else
                    throw new EdgeDBException("Failed to get buffer from the stream");
            }

            var reader = PacketReader.CreateFrom(buff);

            var version = reader.ReadInt64();

            if (version > DumpWriter.DumpVersion)
                throw new ArgumentException($"Unsupported dump version {version}");

            var header = ReadPacket(ref reader);

            if (header is not DumpHeader dumpHeader)
                throw new FormatException($"Expected dump header but got {header?.Type}");

            restore = new Restore()
            {
                HeaderData = dumpHeader.Raw
            };

            while (stream.Position < stream.Length)
            {
                var packet = ReadPacket(ref reader);

                if (packet is not DumpBlock dumpBlock)
                    throw new FormatException($"Expected dump block but got {header?.Type}");

                blocks.Add(new RestoreBlock
                {
                    BlockData = dumpBlock.Raw
                });
            }

            reader.Dispose();

            return (restore!, blocks);
        }

        private static IReceiveable ReadPacket(ref PacketReader reader)
        {
            var type = reader.ReadChar();

            // read hash
            var hash = reader.ReadBytes(20);

            var length = (int)reader.ReadUInt32();

            var packetData = reader.ReadBytes(length);

            // check hash
            if (!SHA1.HashData(packetData.ToArray()).SequenceEqual(hash.ToArray()))
                throw new ArgumentException("Hash did not match");

            var innerReader = PacketReader.CreateFrom(packetData);

            try
            {
                return type switch
                {
                    'H' => new DumpHeader(ref innerReader, length),
                    'D' => new DumpBlock(ref innerReader, length),
                    _ => throw new ArgumentException($"Unknown packet format {type}"),
                };
            }
            finally
            {
                innerReader.Dispose();
            }
        }

        private static void ThrowIfEndOfStream(bool readSuccess)
        {
            if (!readSuccess)
                throw new EndOfStreamException();
        }
    }
}
