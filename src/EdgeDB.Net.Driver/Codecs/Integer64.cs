namespace EdgeDB.Codecs
{
    internal class Integer64 : IScalarCodec<long>
    {
        public long Deserialize(PacketReader reader)
        {
            return reader.ReadInt64();
        }

        public void Serialize(PacketWriter writer, long value)
        {
            writer.Write(value);
        }
    }
}
