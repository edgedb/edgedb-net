namespace EdgeDB.Binary.Codecs
{
    internal sealed class UUIDCodec
        : BaseScalarCodec<Guid>
    {
        public override Guid Deserialize(ref PacketReader reader)
        {
            return reader.ReadGuid();
        }

        public override void Serialize(ref PacketWriter writer, Guid value)
        {
            writer.Write(value);
        }
    }
}
