using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#logmessage">Log Message</see> packet.
    /// </summary>
    internal readonly struct LogMessage : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type => ServerMessageType.LogMessage;

        /// <summary>
        ///     Gets the severity of the log message.
        /// </summary>
        public MessageSeverity Severity { get; }

        /// <summary>
        ///     Gets the error code related to the log message.
        /// </summary>
        public ServerErrorCodes Code { get; }

        /// <summary>
        ///     Gets the content of the log message.
        /// </summary>
        public string Content { get; }

        /// <summary>
        ///     Gets a read-only collection of annotations.
        /// </summary>
        public IReadOnlyCollection<Annotation> Annotations
            => _annotations.ToImmutableArray();

        private readonly Annotation[] _annotations;

        internal LogMessage(ref PacketReader reader)
        {
            Severity = (MessageSeverity)reader.ReadByte();
            Code = (ServerErrorCodes)reader.ReadUInt32();
            Content = reader.ReadString();
            _annotations = reader.ReadAnnotations();
        }
    }
}
