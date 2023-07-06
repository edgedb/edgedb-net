using EdgeDB.Binary.Protocol;
using EdgeDB.Binary.Protocol.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#errorresponse">Error Response</see> packet.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    internal readonly struct ErrorResponse : IReceiveable, IExecuteError, IProtocolError
#pragma warning restore CS0618 // Type or member is obsolete
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

        private readonly KeyValue[] _attributes;

        internal ErrorResponse(ref PacketReader reader)
        {
            Severity = (ErrorSeverity)reader.ReadByte();
            ErrorCode = (ServerErrorCodes)reader.ReadUInt32();
            Message = reader.ReadString();
            _attributes = reader.ReadKeyValues();
        }

        public bool TryGetAttribute(in ushort code, out KeyValue kv)
        {
            for(int i = 0; i != _attributes.Length; i++)
            {
                ref var attr = ref _attributes[i];

                if (attr.Code == code)
                {
                    kv = attr;
                    return true;
                }
            }

            kv = default;
            return false;
        }
    }
}
