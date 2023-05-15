using System.Collections.Immutable;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#data">Data</see> packet
    /// </summary>
    internal readonly struct Data : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.Data;

        internal readonly unsafe ReservedBuffer* Buffer;

        internal unsafe Data(ref PacketReader reader)
        {
            // skip arary since its always one, errr should be one
            var numElements = reader.ReadUInt16();
            if (numElements != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(reader), $"Expected one element array for data, got {numElements}");
            }

            var payloadLength = reader.ReadUInt32();
            Buffer = reader.ReserveRead(payloadLength);
        }
    }
}
