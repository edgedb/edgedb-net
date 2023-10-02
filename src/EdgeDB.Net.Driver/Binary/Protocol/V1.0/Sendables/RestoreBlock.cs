using EdgeDB.Utils;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class RestoreBlock : Sendable
{
    public override int Size
        => BinaryUtils.SizeOfByteArray(BlockData);

    public override ClientMessageTypes Type
        => ClientMessageTypes.RestoreBlock;

    public byte[]? BlockData { get; set; }

    protected override void BuildPacket(ref PacketWriter writer) => writer.WriteArray(BlockData!);
}
