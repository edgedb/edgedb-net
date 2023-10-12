using EdgeDB.Utils;

namespace EdgeDB.Binary;

internal readonly struct ConnectionParam
{
    public int Size
        => BinaryUtils.SizeOfString(Name) + BinaryUtils.SizeOfString(Value);

    public string Name { get; init; }

    public string Value { get; init; }

    public void Write(ref PacketWriter writer)
    {
        writer.Write(Name);
        writer.Write(Value);
    }
}
