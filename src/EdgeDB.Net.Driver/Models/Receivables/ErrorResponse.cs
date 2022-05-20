using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#errorresponse">Error Response</see> packet.
    /// </summary>
    public readonly struct ErrorResponse : IReceiveable, IExecuteError
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
        ///     Gets a collection of headers sent with this error.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; }

        internal ErrorResponse(PacketReader reader)
        {
            Severity = (ErrorSeverity)reader.ReadByte();
            ErrorCode = (ServerErrorCodes)reader.ReadUInt32();
            Message = reader.ReadString();
            Headers = reader.ReadHeaders();
        }

        string? IExecuteError.Message => Message;

        ServerErrorCodes IExecuteError.ErrorCode => ErrorCode;
    }
}
