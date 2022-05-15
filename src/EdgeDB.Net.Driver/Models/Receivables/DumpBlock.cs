using System.Security.Cryptography;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-block">Dump Block</see> packet.
    /// </summary>
    public struct DumpBlock : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.DumpBlock;

        /// <summary>
        ///     Gets the sha1 hash of this packets data, used when writing a dump file.
        /// </summary>
        public IReadOnlyCollection<byte> Hash { get; private set; }

        /// <summary>
        ///     Gets the length of this packets data, used when writing a dump file.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        ///     Gets a collection of headers for this packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        /// <summary>
        ///     Gets a collection of block type headers derived from the <see cref="Headers"/> collection.
        /// </summary>
        public IEnumerable<Header> BlockTypes => Headers.Where(x => x.Code == 101);

        /// <summary>
        ///     Gets a collection of block id headers derived from the <see cref="Headers"/> collection.
        /// </summary>
        public IEnumerable<Header> BlockIds => Headers.Where(x => x.Code == 110);

        /// <summary>
        ///     Gets a collection of block number headers derived from the <see cref="Headers"/> collection.
        /// </summary>
        public IEnumerable<Header> BlockNumbers => Headers.Where(x => x.Code == 111);

        /// <summary>
        ///     Gets a collection of block data headers derived from the <see cref="Headers"/> collection.
        /// </summary>
        public IEnumerable<Header> BlockData => Headers.Where(x => x.Code == 112);

        internal byte[] Raw { get; private set; }
        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            Length = length;

            Raw = reader.ReadBytes((int)length);

            Hash = SHA1.Create().ComputeHash(Raw);

            using (var r = new PacketReader(Raw))
            {
                Headers = r.ReadHeaders();
            }
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }
}
