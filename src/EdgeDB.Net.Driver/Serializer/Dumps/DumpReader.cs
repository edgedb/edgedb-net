using EdgeDB.Models;
using EdgeDB.Utils;
using System.Security.Cryptography;

namespace EdgeDB.Dumps
{
    internal class DumpReader
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

            using (var reader = new PacketReader(stream))
            {
                var version = reader.ReadInt64();

                if (version > DumpWriter.DumpVersion)
                    throw new ArgumentException($"Unsupported dump version {version}");

                var header = ReadPacket(reader);

                if (header is not DumpHeader dumpHeader)
                    throw new FormatException($"Expected dump header but got {header?.Type}");

                restore = new Restore()
                {
                    HeaderData = dumpHeader.Raw
                };

                while (stream.Position < stream.Length)
                {
                    var packet = ReadPacket(reader);

                    if(packet is not DumpBlock dumpBlock)
                        throw new FormatException($"Expected dump block but got {header?.Type}");
                    
                    blocks.Add(new RestoreBlock
                    {
                        BlockData = dumpBlock.Raw
                    });
                }
            }

            return (restore!, blocks);
        }

        private static IReceiveable ReadPacket(PacketReader reader)
        {
            var type = reader.ReadChar();

            IReceiveable packet = type switch
            {
                'H' => new DumpHeader(),
                'D' => new DumpBlock(),
                _ => throw new ArgumentException($"Unknown packet format {type}"),
            };

            // read hash
            var hash = reader.ReadBytes(20);

            var length = reader.ReadUInt32();

            var packetData = reader.ReadBytes((int)length);

            // check hash
            using (var alg = SHA1.Create())
            {
                if (!alg.ComputeHash(packetData).SequenceEqual(hash))
                    throw new ArgumentException("Hash did not match");
            }

            using (var innerReader = new PacketReader(packetData))
            {
                packet.Read(innerReader, length, null!);
            }

            return packet;
        }

        private static void ThrowIfEndOfStream(bool readSuccess)
        {
            if (!readSuccess)
                throw new EndOfStreamException();
        }
    }
}
