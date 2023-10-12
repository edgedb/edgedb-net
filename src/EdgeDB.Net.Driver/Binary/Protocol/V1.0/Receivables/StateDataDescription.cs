namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#statedatadescription">State Data Description</see>
///     packet.
/// </summary>
internal readonly struct StateDataDescription : IReceiveable
{
    public ServerMessageType Type => ServerMessageType.StateDataDescription;

    public readonly Guid TypeDescriptorId;

    internal readonly byte[] TypeDescriptorBuffer;

    internal StateDataDescription(ref PacketReader reader)
    {
        TypeDescriptorId = reader.ReadGuid();
        TypeDescriptorBuffer = reader.ReadByteArray();
    }
}
