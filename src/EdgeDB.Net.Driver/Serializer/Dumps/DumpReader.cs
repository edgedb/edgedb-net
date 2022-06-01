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

            Span<byte> buff;
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                if (ms.TryGetBuffer(out var arr))
                    buff = arr;
                else
                    throw new EdgeDBException("Failed to get buffer from the stream");
            }

            using (var reader = new PacketReader(buff))
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

            // read hash
            reader.ReadBytes(20, out var hash);

            var length = reader.ReadUInt32();

            reader.ReadBytes((int)length, out var packetData);

            // check hash
            using (var alg = SHA1.Create())
            {
                if (!alg.ComputeHash(packetData.ToArray()).SequenceEqual(hash.ToArray()))
                    throw new ArgumentException("Hash did not match");
            }

            using (var innerReader = new PacketReader(packetData))
            {
                return type switch
                {
                    'H' => new DumpHeader(innerReader, length),
                    'D' => new DumpBlock(innerReader, length),
                    _ => throw new ArgumentException($"Unknown packet format {type}"),
                };
            }
        }

        private static void ThrowIfEndOfStream(bool readSuccess)
        {
            if (!readSuccess)
                throw new EndOfStreamException();
        }
    }
}
