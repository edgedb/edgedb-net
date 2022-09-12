using System.Collections.Immutable;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#data">Data</see> packet
    /// </summary>
    public readonly struct Data : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.Data;

        /// <summary>
        ///     Gets the payload of this data packet
        /// </summary>
        public IReadOnlyCollection<byte> PayloadData
            => PayloadData.ToImmutableArray();

        internal readonly byte[] PayloadBuffer;

        internal Data(byte[] buff)
        {
            PayloadBuffer = buff;
        }
        public Data()
        {
            PayloadBuffer = new byte[] { };
        }

        internal Data(ref PacketReader reader)
        {
            // skip arary since its always one, errr should be one
            var numElements = reader.ReadUInt16();
            if (numElements != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(reader), $"Expected one element array for data, got {numElements}");
            }

            var payloadLength = reader.ReadUInt32();
            reader.ReadBytes((int)payloadLength, out var buff);
            PayloadBuffer = buff.ToArray();
        }
    }
}
