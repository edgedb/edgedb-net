namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationok">AuthenticationOK</see>,
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsasl">AuthenticationSASL</see>,
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsaslcontinue">AuthenticationSASLContinue</see>
///     ,
///     and
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#authenticationsaslfinal">AuthenticationSASLFinal</see>
///     packets.
/// </summary>
internal readonly struct AuthenticationStatus : IReceiveable
{
    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.Authentication;

    /// <summary>
    ///     The authentication state.
    /// </summary>
    public readonly AuthStatus AuthStatus;

    /// <summary>
    ///     A collection of supported authentication methods.
    /// </summary>
    public readonly string[]? AuthenticationMethods;

    /// <summary>
    ///     The SASL data.
    /// </summary>
    internal readonly byte[] SASLDataBuffer;

    internal AuthenticationStatus(ref PacketReader reader)
    {
        AuthStatus = (AuthStatus)reader.ReadUInt32();

        switch (AuthStatus)
        {
            case AuthStatus.AuthenticationRequiredSASLMessage:
            {
                var count = reader.ReadUInt32();
                AuthenticationMethods = new string[count];

                for (var i = 0; i != count; i++)
                {
                    AuthenticationMethods[i] = reader.ReadString();
                }

                SASLDataBuffer = Array.Empty<byte>();
            }
                break;
            case AuthStatus.AuthenticationSASLContinue or AuthStatus.AuthenticationSASLFinal:
            {
                SASLDataBuffer = reader.ReadByteArray();
                AuthenticationMethods = Array.Empty<string>();
            }
                break;
            default:
                SASLDataBuffer = Array.Empty<byte>();
                AuthenticationMethods = Array.Empty<string>();
                break;
        }
    }
}
