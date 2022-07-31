using System.Collections.Immutable;
using System.Security.Cryptography;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-block">Dump Block</see> packet.
    /// </summary>
    public readonly struct DumpBlock : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.DumpBlock;

        /// <summary>
        ///     Gets the sha1 hash of this packets data, used when writing a dump file.
        /// </summary>
        public IReadOnlyCollection<byte> Hash
            => HashBuffer.ToImmutableArray();

        /// <summary>
        ///     Gets the length of this packets data, used when writing a dump file.
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     Gets a collection of attributes for this packet.
        /// </summary>
        public IReadOnlyCollection<Annotation> Attributes
            => _attributes.ToImmutableArray();

        internal byte[] Raw { get; }

        internal byte[] HashBuffer { get; }

        private readonly Annotation[] _attributes;

        internal DumpBlock(ref PacketReader reader, in int length)
        {
            Length = length;

            reader.ReadBytes(length, out var rawBuff);
            Raw = rawBuff.ToArray();

            HashBuffer = SHA1.Create().ComputeHash(Raw);

            using var r = new PacketReader(rawBuff);
            _attributes = r.ReadKeyValues();
        }
    }
}
