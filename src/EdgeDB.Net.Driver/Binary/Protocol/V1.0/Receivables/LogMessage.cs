using EdgeDB.Binary.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#logmessage">Log Message</see> packet.
    /// </summary>
    internal readonly struct LogMessage : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.LogMessage;

        /// <summary>
        ///     The severity of the log message.
        /// </summary>
        public readonly MessageSeverity Severity;

        /// <summary>
        ///     The error code related to the log message.
        /// </summary>
        public readonly ServerErrorCodes Code;

        /// <summary>
        ///     The content of the log message.
        /// </summary>
        public readonly string Content;

        /// <summary>
        ///     A collection of annotations.
        /// </summary>
        public readonly Annotation[] Annotations;

        internal LogMessage(ref PacketReader reader)
        {
            Severity = (MessageSeverity)reader.ReadByte();
            Code = (ServerErrorCodes)reader.ReadUInt32();
            Content = reader.ReadString();
            Annotations = reader.ReadAnnotations();
        }
    }
}
