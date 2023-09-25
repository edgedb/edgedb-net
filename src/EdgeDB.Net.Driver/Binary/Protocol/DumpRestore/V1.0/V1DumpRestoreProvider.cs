using EdgeDB.Binary.Protocol.V1._0.Packets;
using EdgeDB.Utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
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

        public async Task DumpDatabaseAsync(EdgeDBBinaryClient client, Stream stream, CancellationToken token = default(CancellationToken))
        {
            using var cmdLock = await client.AquireCommandLockAsync(token).ConfigureAwait(false);

            try
            {
                var state = new DumpState(stream);

                await foreach (var result in client.Duplexer.DuplexAndSyncAsync(new Dump(), token))
                {
                    switch (result.Packet)
                    {
                        case ReadyForCommand:
                            result.Finish();
                            break;
                        case DumpHeader dumpHeader:
                            await state.WriteHeaderAsync(in dumpHeader);
                            break;
                        case DumpBlock block:
                            await state.WriteBlockAsync(in block);
                            break;
                        case ErrorResponse error:
                            {
                                throw new EdgeDBErrorException(error);
                            }
                    }
                }
            }
            catch (Exception x) when (x is OperationCanceledException or TaskCanceledException)
            {
                throw new TimeoutException("Database dump timed out", x);
            }
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

        public unsafe (Restore Restore, IEnumerable<RestoreBlock> Blocks) ReadDatabaseDump(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException($"Cannot read from {nameof(stream)}");

            // read file format
            Span<byte> formatBuffer = stackalloc byte[DUMP_FILE_FORMAT_LENGTH];

            ThrowIfEndOfStream(stream.Read(formatBuffer) == DUMP_FILE_FORMAT_LENGTH);

            if (!formatBuffer.SequenceEqual(DUMP_FILE_FORMAT_BLOB))
                throw new FormatException("Format of stream does not match the edgedb dump format");

            Restore? restore = null;
            List<RestoreBlock> blocks = new();

            long version = 0;

            BinaryUtils.ReadPrimitive(stream, ref version);

            
            if (version > _version.Major)
                throw new ArgumentException($"Unsupported dump version {version}");

            var header = ReadPacket(stream);

            if (header is not DumpHeader dumpHeader)
                throw new FormatException($"Expected dump header but got {header?.Type}");

            restore = new Restore()
            {
                HeaderData = dumpHeader.Raw
            };

            while (stream.Position < stream.Length)
            {
                var packet = ReadPacket(stream);

                if (packet is not DumpBlock dumpBlock)
                    throw new FormatException($"Expected dump block but got {header?.Type}");

                blocks.Add(new RestoreBlock
                {
                    BlockData = dumpBlock.Raw
                });
            }

            return (restore!, blocks);
        }

        private unsafe static IReceiveable ReadPacket(Stream stream)
        {
            scoped Span<byte> headerBuffer = stackalloc byte[sizeof(DumpSectionHeader)];

            stream.Read(headerBuffer);

            ref var header = ref Unsafe.As<byte, DumpSectionHeader>(ref headerBuffer[0]);

            BinaryUtils.CorrectEndianness(ref header.Length);

            // read packet data
            var data = new byte[header.Length];

            stream.Read(data);

            // check hash
            using(var alg = SHA1.Create())
            {
                if(!alg.ComputeHash(data).SequenceEqual(header.Hash.ToArray()))
                    throw new ArgumentException("Hash did not match");
            }

            var reader = new PacketReader(data);

            return header.Type switch
            {
                'H' => new DumpHeader(ref reader, in header.Length),
                'D' => new DumpBlock(ref reader, in header.Length),
                _ => throw new ArgumentException($"Unknown packet format {header.Type}")
            };
        }

        private static void ThrowIfEndOfStream(bool readSuccess)
        {
            if (!readSuccess)
                throw new EndOfStreamException();
        }
    }
}
