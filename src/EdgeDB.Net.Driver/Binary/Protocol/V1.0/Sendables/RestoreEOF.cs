namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class RestoreEOF : Sendable
{
    public override int Size => 0;

    public override ClientMessageTypes Type
        => ClientMessageTypes.RestoreEOF;

    protected override void BuildPacket(ref PacketWriter writer)
    {
        // write nothing
    }
}
