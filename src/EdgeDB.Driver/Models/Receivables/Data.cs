using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#data">Data</see> packet
    /// </summary>
    public struct Data : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.Data;

        /// <summary>
        ///     Gets the payload of this data packet
        /// </summary>
        public IReadOnlyCollection<byte> PayloadData { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            // skip arary since its always one, errr should be one
            var numElements = reader.ReadUInt16();
            if (numElements != 1)
            {
                throw new Exception($"Expected one element array for data, got {numElements}");
            }

            var payloadLength = reader.ReadUInt32();

            PayloadData = reader.ReadBytes((int)payloadLength); 
        }
    }
}
