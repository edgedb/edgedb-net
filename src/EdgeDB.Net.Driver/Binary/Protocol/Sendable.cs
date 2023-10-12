namespace EdgeDB.Binary.Protocol;

internal abstract class Sendable
{
    public abstract int Size { get; }

    public abstract ClientMessageTypes Type { get; }

    protected abstract void BuildPacket(ref PacketWriter writer);

    public void Write(ref PacketWriter writer)
    {
        // advance 5 bytes
        var start = writer.Index;
        writer.Advance(5);

        // write the body of the packet
        BuildPacket(ref writer);

        // store the index after writing the body
        var eofPosition = writer.Index;

        // seek back to the beginning.
        writer.SeekToIndex(start);

        // write the type and size
        writer.Write((sbyte)Type);
        writer.Write((uint)Size + 4);

        // go back to eof
        writer.SeekToIndex(eofPosition);
    }

    public int GetSize()
        => Size + 5;
}
