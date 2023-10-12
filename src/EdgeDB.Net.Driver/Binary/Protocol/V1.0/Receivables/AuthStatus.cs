namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the authentication state.
/// </summary>
internal enum AuthStatus : uint
{
    /// <summary>
    ///     The authentication was successful and is validated.
    /// </summary>
    AuthenticationOK = 0x0,

    /// <summary>
    ///     The server requires an SASL message.
    /// </summary>
    AuthenticationRequiredSASLMessage = 0xa,

    /// <summary>
    ///     The client should continue sending SASL messages.
    /// </summary>
    AuthenticationSASLContinue = 0xb,

    /// <summary>
    ///     The received message was the final authentication message.
    /// </summary>
    AuthenticationSASLFinal = 0xc
}
