namespace EdgeDB.Binary.Protocol.V1._0.Packets;

/// <summary>
///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#data">Data</see> packet
/// </summary>
internal readonly struct Data : IReceiveable
{
    /// <inheritdoc />
    public ServerMessageType Type
        => ServerMessageType.Data;

    /// <summary>
    ///     The payload of this data packet
    /// </summary>
    public readonly byte[] PayloadBuffer;

    internal Data(byte[] buff)
    {
        PayloadBuffer = buff;
    }

    public Data()
    {
        PayloadBuffer = Array.Empty<byte>();
    }

    internal Data(ref PacketReader reader)
    {
        // skip arary since its always one, errr should be one
        var numElements = reader.ReadUInt16();
        if (numElements != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(reader),
                $"Expected one element array for data, got {numElements}");
        }

        var payloadLength = reader.ReadUInt32();
        reader.ReadBytes((int)payloadLength, out var buff);
        PayloadBuffer = buff.ToArray();
    }
}
