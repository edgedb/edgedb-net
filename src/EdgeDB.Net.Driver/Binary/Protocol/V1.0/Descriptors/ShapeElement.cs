using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors;

internal readonly struct ShapeElement
{
    public readonly ShapeElementFlags Flags;
    public readonly Cardinality Cardinality;
    public readonly string Name;
    public readonly ushort TypePos;

    public ShapeElement(ref PacketReader reader)
    {
        Flags = (ShapeElementFlags)reader.ReadUInt32();
        Cardinality = (Cardinality)reader.ReadByte();
        Name = reader.ReadString();
        TypePos = reader.ReadUInt16();
    }
}
