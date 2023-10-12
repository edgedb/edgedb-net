namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the
///     <see href="https://www.edgedb.com/docs/reference/protocol/messages#commanddatadescription">Command Data Description</see>
///     packet.
/// </summary>
internal readonly struct CommandDataDescription : IReceiveable
{
    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.CommandDataDescription;

    public readonly Capabilities Capabilities;

    /// <summary>
    ///     The cardinality of the command.
    /// </summary>
    public readonly Cardinality Cardinality;

    /// <summary>
    ///     The input type descriptor id.
    /// </summary>
    public readonly Guid InputTypeDescriptorId;

    /// <summary>
    ///     The complete input type descriptor.
    /// </summary>
    /// <summary>
    ///     The output type descriptor id.
    /// </summary>
    public readonly Guid OutputTypeDescriptorId;

    public readonly Annotation[] Annotations;

    /// <summary>
    ///     The complete input type descriptor.
    /// </summary>
    public readonly byte[] InputTypeDescriptorBuffer;

    /// <summary>
    ///     The complete output type descriptor.
    /// </summary>
    public readonly byte[] OutputTypeDescriptorBuffer;

    internal CommandDataDescription(ref PacketReader reader)
    {
        Annotations = reader.ReadAnnotations();
        Capabilities = (Capabilities)reader.ReadUInt64();
        Cardinality = (Cardinality)reader.ReadByte();
        InputTypeDescriptorId = reader.ReadGuid();
        InputTypeDescriptorBuffer = reader.ReadByteArray();
        OutputTypeDescriptorId = reader.ReadGuid();
        OutputTypeDescriptorBuffer = reader.ReadByteArray();
    }
}
