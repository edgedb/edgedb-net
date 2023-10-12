using EdgeDB.Binary.Protocol.Common;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#errorresponse">Error Response</see> packet.
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
internal readonly struct ErrorResponse : IReceiveable, IExecuteError, IProtocolError
#pragma warning restore CS0618 // Type or member is obsolete
{
    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.ErrorResponse;

    /// <summary>
    ///     The severity of the error.
    /// </summary>
    public readonly ErrorSeverity Severity;

    /// <summary>
    ///     The error code.
    /// </summary>
    public readonly ServerErrorCodes ErrorCode;

    /// <summary>
    ///     The message of the error.
    /// </summary>
    public readonly string Message;

    public readonly KeyValue[] Attributes;

    internal ErrorResponse(ref PacketReader reader)
    {
        Severity = (ErrorSeverity)reader.ReadByte();
        ErrorCode = (ServerErrorCodes)reader.ReadUInt32();
        Message = reader.ReadString();
        Attributes = reader.ReadKeyValues();
    }

    public bool TryGetAttribute(in ushort code, out KeyValue kv)
    {
        for (var i = 0; i != Attributes.Length; i++)
        {
            ref var attr = ref Attributes[i];

            if (attr.Code == code)
            {
                kv = attr;
                return true;
            }
        }

        kv = default;
        return false;
    }

    ErrorSeverity IProtocolError.Severity => Severity;

    ServerErrorCodes IProtocolError.ErrorCode => ErrorCode;

    string IProtocolError.Message => Message;

    string? IExecuteError.Message => Message;

    ServerErrorCodes IExecuteError.ErrorCode => ErrorCode;
}
