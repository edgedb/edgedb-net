using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#commandcomplete">Command Complete</see> packet
    /// </summary>
    public struct CommandComplete : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.CommandComplete;

        /// <summary>
        ///     Gets the used capabilities within the completed command.
        /// </summary>
        public AllowCapabilities UsedCapabilities { get; private set; }

        /// <summary>
        ///     Gets the status of the completed command.
        /// </summary>
        public string Status { get; private set; }

        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            var headers = reader.ReadHeaders();

            var usedCap = headers.Cast<Header?>().FirstOrDefault(x => x!.Value.Code == 0x1001);

            if (usedCap.HasValue)
            {
                UsedCapabilities = (AllowCapabilities)ICodec.GetScalarCodec<long>()!.Deserialize(usedCap.Value.Value);
            }

            Status = reader.ReadString();
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }
}
