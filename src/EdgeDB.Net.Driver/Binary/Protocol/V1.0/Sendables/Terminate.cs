namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class Terminate : Sendable
{
    public override int Size => 0;

    public override ClientMessageTypes Type
        => ClientMessageTypes.Terminate;

    protected override void BuildPacket(ref PacketWriter writer) { } // no data
}
