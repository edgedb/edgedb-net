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
    public struct ErrorResponse : IReceiveable, IExecuteError
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.ErrorResponse;

        /// <summary>
        ///     Gets the severity of the error.
        /// </summary>
        public ErrorSeverity Severity { get; private set; }

        /// <summary>
        ///     Gets the error code.
        /// </summary>
        public uint ErrorCode { get; private set; }

        /// <summary>
        ///     Gets the message of the error.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     Gets a collection of headers sent with this error.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; private set; }

        ulong IReceiveable.Id { get; set; }

        void IReceiveable.Read(PacketReader reader, uint length, EdgeDBBinaryClient client)
        {
            Severity = (ErrorSeverity)reader.ReadByte();
            ErrorCode = reader.ReadUInt32();
            Message = reader.ReadString();
            Headers = reader.ReadHeaders();
        }

        IReceiveable IReceiveable.Clone()
            => (IReceiveable)MemberwiseClone();

        string? IExecuteError.Message => Message;

        uint IExecuteError.ErrorCode => ErrorCode;
    }
}
