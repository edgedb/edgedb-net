namespace EdgeDB.Binary;

/// <summary>
///     Represents all supported message types sent by the server.
/// </summary>
internal enum ServerMessageType : sbyte
{
    RestoreReady = 0x2b,
    DumpBlock = 0x3d,
    DumpHeader = 0x40,
    CommandComplete = 0x43,
    Data = 0x44,
    ErrorResponse = 0x45,
    ServerKeyData = 0x4b,
    LogMessage = 0x4c,
    Authentication = 0x52,
    CommandDataDescription = 0x54,
    ParameterStatus = 0x53,
    ReadyForCommand = 0x5a,
    StateDataDescription = 0x73,
    ServerHandshake = 0x76
}
