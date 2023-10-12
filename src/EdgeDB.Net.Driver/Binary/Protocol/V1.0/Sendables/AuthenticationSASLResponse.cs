using EdgeDB.Utils;
using System.Text;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class AuthenticationSASLResponse : Sendable
{
    private readonly ReadOnlyMemory<byte> _payload;

    public AuthenticationSASLResponse(ReadOnlyMemory<byte> payload)
    {
        _payload = payload;
    }

    public override ClientMessageTypes Type
        => ClientMessageTypes.AuthenticationSASLResponse;

    public override int Size
        => BinaryUtils.SizeOfByteArray(_payload);

    protected override void BuildPacket(ref PacketWriter writer) => writer.WriteArray(_payload);

    public override string ToString() => Encoding.UTF8.GetString(_payload.Span);
}
