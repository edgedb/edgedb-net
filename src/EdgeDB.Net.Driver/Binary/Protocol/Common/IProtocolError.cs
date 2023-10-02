namespace EdgeDB.Binary.Protocol.Common;

internal interface IProtocolError
{
    ErrorSeverity Severity { get; }
    ServerErrorCodes ErrorCode { get; }
    string Message { get; }

    bool TryGetAttribute(in ushort code, out KeyValue kv);
}
