using EdgeDB.Utils;
using System.Text;

namespace EdgeDB.Binary.Protocol.V1._0.Packets;

internal sealed class AuthenticationSASLInitialResponse : Sendable
{
    public AuthenticationSASLInitialResponse(ReadOnlyMemory<byte> payload, string method)
    {
        Method = method;
        Payload = payload;
    }

    public override ClientMessageTypes Type
        => ClientMessageTypes.AuthenticationSASLInitialResponse;

    public override int Size
        => BinaryUtils.SizeOfString(Method) + BinaryUtils.SizeOfByteArray(Payload);

    public string Method { get; set; }

    public ReadOnlyMemory<byte> Payload { get; set; }

    protected override void BuildPacket(ref PacketWriter writer)
    {
        writer.Write(Method);
        writer.WriteArray(Payload);
    }

    public override string ToString() => $"{Method} {Encoding.UTF8.GetString(Payload.Span)}";
}
