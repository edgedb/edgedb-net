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
    public readonly struct ParseComplete : IReceiveable
    {
        public const int CAPBILITIES_HEADER = 0x1001;

        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ParseComplete;

        /// <summary>
        ///     Gets the allowed capabilities that the command will actually use.
        /// </summary>
        public Capabilities? Capabilities { get; }

        /// <summary>
        ///     Gets the cardinality the command will return.
        /// </summary>
        public Cardinality Cardinality { get; }

        /// <summary>
        ///     Gets the input type descriptor id.
        /// </summary>
        public Guid InputTypedescId { get; }

        /// <summary>
        ///     Gets the output type descriptor id.
        /// </summary>
        public Guid OutputTypedescId { get; }

        internal ParseComplete(PacketReader reader)
        {
            var headers = reader.ReadHeaders();
            Capabilities = null;
            for (int i = 0; i != headers.Length; i++)
            {
                if (headers[i].Code == CAPBILITIES_HEADER)
                {
                    Capabilities = (Capabilities)ICodec.GetScalarCodec<long>()!.Deserialize(headers[i].Value);
                }
            }

            Cardinality = (Cardinality)reader.ReadByte();
            InputTypedescId = reader.ReadGuid();
            OutputTypedescId = reader.ReadGuid();
        }
    }
}
