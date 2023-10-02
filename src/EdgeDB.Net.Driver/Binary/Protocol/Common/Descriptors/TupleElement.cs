namespace EdgeDB.Binary.Protocol.Common.Descriptors;

internal readonly struct TupleElement
{
    public readonly string Name;

    public readonly short TypePos;

    public TupleElement(scoped ref PacketReader reader)
    {
        Name = reader.ReadString();
        TypePos = reader.ReadInt16();
    }
}
