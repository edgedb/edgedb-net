using EdgeDB.Binary.Protocol;

namespace EdgeDB.Binary.Protocol.V1._0.Packets
{
    internal sealed class ClientHandshake : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.ClientHandshake;
        public override int Size
        {
            get
            {
                return (sizeof(short) << 2) + ConnectionParameters.Sum(x => x.Size) + Extensions.Sum(x => x.Size);
            }
        }

            
        public ushort MajorVersion { get; set; }
        public ushort MinorVersion { get; set; }
        public ConnectionParam[] ConnectionParameters { get; set; } = Array.Empty<ConnectionParam>();
        public ProtocolExtension[] Extensions { get; set; } = Array.Empty<ProtocolExtension>();

        protected override void BuildPacket(ref PacketWriter writer)
        {
            writer.Write(MajorVersion);
            writer.Write(MinorVersion);

            writer.Write((ushort)ConnectionParameters.Length);
            foreach (var param in ConnectionParameters)
            {
                param.Write(ref writer);
            }

            writer.Write((ushort)Extensions.Length);
            foreach (var extension in Extensions)
            {
                extension.Write(ref writer);
            }
        }
    }
}
