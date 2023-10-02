using EdgeDB.Utils;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class Dump : Sendable
{
    public override int Size => BinaryUtils.SizeOfAnnotations(Attributes);

    public override ClientMessageTypes Type
        => ClientMessageTypes.Dump;

    public Annotation[]? Attributes { get; set; }

    protected override void BuildPacket(ref PacketWriter writer) => writer.Write(Attributes);
}
