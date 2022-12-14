using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#errorresponse">Error Response</see> packet.
    /// </summary>
    internal readonly struct ErrorResponse : IReceiveable, IExecuteError
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ErrorResponse;

        /// <summary>
        ///     Gets the severity of the error.
        /// </summary>
        public ErrorSeverity Severity { get; }

        /// <summary>
        ///     Gets the error code.
        /// </summary>
        public ServerErrorCodes ErrorCode { get; }

        /// <summary>
        ///     Gets the message of the error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets a collection of attributes sent with this error.
        /// </summary>
        public IReadOnlyCollection<KeyValue> Attributes
            => _attributes.ToImmutableArray();

        private readonly KeyValue[] _attributes;

        internal ErrorResponse(ref PacketReader reader)
        {
            Severity = (ErrorSeverity)reader.ReadByte();
            ErrorCode = (ServerErrorCodes)reader.ReadUInt32();
            Message = reader.ReadString();
            _attributes = reader.ReadKeyValues();
        }

        internal bool TryGetAttribute(ushort code, out KeyValue value)
        {
            value = _attributes.FirstOrDefault(x => x.Code == code);
            return _attributes.Any(x => x.Code == code);
        }

        string? IExecuteError.Message => Message;

        ServerErrorCodes IExecuteError.ErrorCode => ErrorCode;
    }
}
