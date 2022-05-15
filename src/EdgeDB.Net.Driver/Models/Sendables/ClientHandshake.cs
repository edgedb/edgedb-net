namespace EdgeDB.Models
{
    internal class ClientHandshake : Sendable
    {
        public override ClientMessageTypes Type => ClientMessageTypes.ClientHandshake;

        public short MajorVersion { get; set; }
        public short MinorVersion { get; set; }
        public ConnectionParam[] ConnectionParameters { get; set; } = Array.Empty<ConnectionParam>();
        public ProtocolExtension[] Extensions { get; set; } = Array.Empty<ProtocolExtension>();

        protected override void BuildPacket(PacketWriter writer, EdgeDBTcpClient client)
        {
            writer.Write(MajorVersion);
            writer.Write(MinorVersion);

            writer.Write((ushort)ConnectionParameters.Length);
            foreach (var param in ConnectionParameters)
            {
                param.Write(writer);
            }

            writer.Write(Extensions.Length);
            foreach (var extension in Extensions)
            {
                extension.Write(writer);
            }
        }
    }
}
