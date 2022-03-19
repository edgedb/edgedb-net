using EdgeDB.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#preparecomplete">Prepare Complete</see> packet
    /// </summary>
    public struct PrepareComplete : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.PrepareComplete;

        /// <summary>
        ///     Gets the allowed capabilities that the command will actually use.
        /// </summary>
        public AllowCapabilities Capabilities { get; private set; }

        /// <summary>
        ///     Gets the cardinality the command will return.
        /// </summary>
        public Cardinality Cardinality { get; private set; }

        /// <summary>
        ///     Gets the input type descriptor id.
        /// </summary>
        public Guid InputTypedescId { get; private set; }

        /// <summary>
        ///     Gets the output type descriptor id.
        /// </summary>
        public Guid OutputTypedescId { get; private set; }

        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBTcpClient client)
        {
            var headers = reader.ReadHeaders();

            var capabilities = headers.Cast<Header?>().FirstOrDefault(x => x!.Value.Code == 0x1001);

            if(capabilities != null)
            {
                Capabilities = (AllowCapabilities)ICodec.GetScalarCodec<long>()!.Deserialize(capabilities.Value.Value);
            }

            Cardinality = (Cardinality)reader.ReadByte();
            InputTypedescId = reader.ReadGuid();
            OutputTypedescId = reader.ReadGuid();
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();
    }
}
