using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commandcomplete">Command Complete</see> packet
    /// </summary>
    public readonly struct CommandComplete : IReceiveable
    {
        public const int CAPBILITIES_HEADER = 0x1001;

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.CommandComplete;

        /// <summary>
        ///     Gets the used capabilities within the completed command.
        /// </summary>
        public Capabilities? UsedCapabilities { get; }

        /// <summary>
        ///     Gets the status of the completed command.
        /// </summary>
        public string Status { get; }

        internal CommandComplete(ref PacketReader reader)
        {
            UsedCapabilities = null;

            var headers = reader.ReadHeaders();
            for (int i = 0; i != headers.Length; i++)
            {
                if (headers[i].Code == CAPBILITIES_HEADER)
                {
                    UsedCapabilities = (Capabilities)ICodec.GetScalarCodec<long>()!.Deserialize(headers[i].Value);
                }
            }

            Status = reader.ReadString();
        }
    }
}
