namespace EdgeDB.Binary.Codecs
{
    internal class UUID : IScalarCodec<Guid>
    {
        public Guid Deserialize(ref PacketReader reader)
        {
            return reader.ReadGuid();
        }

        public void Serialize(ref PacketWriter writer, Guid value)
        {
            writer.Write(value);
        }
    }
}
