using EdgeDB.Utils;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class Restore : Sendable
{
    public override int Size
        => BinaryUtils.SizeOfAnnotations(Headers) + sizeof(ushort) + BinaryUtils.SizeOfByteArray(HeaderData);

    public override ClientMessageTypes Type
        => ClientMessageTypes.Restore;

    public Annotation[]? Headers { get; set; }

    public ushort Jobs { get; } = 1;

    public byte[]? HeaderData { get; set; }

    protected override void BuildPacket(ref PacketWriter writer)
    {
        writer.Write(Headers);
        writer.Write(Jobs);
        writer.WriteArrayWithoutLength(HeaderData!);
    }
}
