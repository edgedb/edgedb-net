namespace EdgeDB.Codecs
{
    internal class UUID : IScalarCodec<Guid>
    {
        public Guid Deserialize(ref PacketReader reader)
        {
            return reader.ReadGuid();
        }

        public void Serialize(PacketWriter writer, Guid value)
        {
            writer.Write(value);
        }
    }
}
