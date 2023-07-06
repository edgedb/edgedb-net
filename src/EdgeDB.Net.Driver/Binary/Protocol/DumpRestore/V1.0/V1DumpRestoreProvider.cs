using EdgeDB.Binary.Protocol.V1._0.Packets;
using EdgeDB.Dumps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.DumpRestore.V1._0
{
    internal class V1DumpRestoreProvider : IDumpRestoreProvider
    {
        public const int DUMP_FILE_FORMAT_LENGTH = 17;

        public static readonly byte[] DUMP_FILE_FORMAT_BLOB = new byte[]
        {
            0xff, 0xd8, 0x00,0x00, 0xd8,

            // file format "EDGEDB"
            69, 68, 71, 69, 68, 66,

            0x00,

            // file format "DUMP
            68, 85, 77, 80,

            0x00,
        };

        public ProtocolVersion DumpRestoreVersion
            => _version;

        private readonly ProtocolVersion _version = (1, 0);

        public Task DumpDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token = default(CancellationToken))
        {

        }

        public async Task<string> RestoreDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token)
        {
            using var cmdLock = await client.AquireCommandLockAsync(token).ConfigureAwait(false);

            var count = await client
                .QueryRequiredSingleAsync<long>("""
                select count(
                    schema::Module
                    filter not .builtin and not .name = "default"
                ) + count(
                    schema::Object
                    filter .name like "default::%"
                )
                """, token: token).ConfigureAwait(false);

            if (count > 0)
                throw new InvalidOperationException("Cannot restore: Database isn't empty");

            var packets = ReadDatabaseDump(stream);

            await foreach (var result in client.Duplexer.DuplexAsync(packets.Restore, token))
            {
                switch (result.Packet)
                {
                    case ErrorResponse err:
                        throw new EdgeDBErrorException(err);
                    case RestoreReady:
                        result.Finish();
                        break;
                    default:
                        throw new UnexpectedMessageException(ServerMessageType.RestoreReady, result.Packet.Type);
                }
            }

            foreach (var block in packets.Blocks)
            {
                await client.Duplexer.SendAsync(block, token).ConfigureAwait(false);
            }

            var restoreResult = await client.Duplexer.DuplexSingleAsync(new RestoreEOF(), token).ConfigureAwait(false);

            return restoreResult is null
                ? throw new UnexpectedDisconnectException()
                : restoreResult is ErrorResponse error
                    ? throw new EdgeDBErrorException(error)
                    : restoreResult is not CommandComplete complete
                        ? throw new UnexpectedMessageException(ServerMessageType.CommandComplete, restoreResult.Type)
                        : complete.Status;
        }

        private static byte[] Version
            => BitConverter.GetBytes((long)1).Reverse().ToArray();

        public static void WriteDumpHeader(ref PacketWriter writer, in DumpHeader header)
        {
            writer.Write('H');
            writer.Write(header.Hash.ToArray());
            writer.Write(header.Length);
            writer.Write(header.Raw);
        }

        public void WriteDumpBlocks(ref PacketWriter writer, DumpBlock[] blocks)
        {
            for (int i = 0; i != blocks.Length; i++)
            {
                ref var block = ref blocks[i];

                writer.Write('D');
                writer.Write(block.HashBuffer);
                writer.Write(block.Length);
                writer.Write(block.Raw);
            }
        }

        public static (Restore Restore, IEnumerable<RestoreBlock> Blocks) ReadDatabaseDump(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException($"Cannot read from {nameof(stream)}");

            // read file format
            var format = new byte[DUMP_FILE_FORMAT_LENGTH];
            ThrowIfEndOfStream(stream.Read(format) == DUMP_FILE_FORMAT_LENGTH);

            if (!format.SequenceEqual(DUMP_FILE_FORMAT_BLOB))
                throw new FormatException("Format of stream does not match the edgedb dump format");

            Restore? restore = null;
            List<RestoreBlock> blocks = new();


            // TODO: buffered reads
            Span<byte> buff;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                if (ms.TryGetBuffer(out var arr))
                    buff = arr;
                else
                    throw new EdgeDBException("Failed to get buffer from the stream");
            }

            var reader = new PacketReader(ref buff);

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

            return (restore!, blocks);
        }

        private static IReceiveable ReadPacket(ref PacketReader reader)
        {
            var type = reader.ReadChar();

            // read hash
            reader.ReadBytes(20, out var hash);

            var length = (int)reader.ReadUInt32();

            reader.ReadBytes(length, out var packetData);

            // check hash
            using (var alg = SHA1.Create())
            {
                if (!alg.ComputeHash(packetData.ToArray()).SequenceEqual(hash.ToArray()))
                    throw new ArgumentException("Hash did not match");
            }

            var innerReader = new PacketReader(ref packetData);

            return type switch
            {
                'H' => new DumpHeader(ref innerReader, length),
                'D' => new DumpBlock(ref innerReader, length),
                _ => throw new ArgumentException($"Unknown packet format {type}"),
            };
        }

        private static void ThrowIfEndOfStream(bool readSuccess)
        {
            if (!readSuccess)
                throw new EndOfStreamException();
        }
    }
}
