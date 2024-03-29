namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#serverkeydata">Server Key Data</see> packet.
/// </summary>
internal readonly struct ServerKeyData : IReceiveable
{
    public const int SERVER_KEY_LENGTH = 32;

    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.ServerKeyData;

    /// <summary>
    ///     The key data buffer.
    /// </summary>
    internal readonly byte[] KeyBuffer;

    internal ServerKeyData(ref PacketReader reader)
    {
        reader.ReadBytes(SERVER_KEY_LENGTH, out var buff);
        KeyBuffer = buff.ToArray();
    }
}
