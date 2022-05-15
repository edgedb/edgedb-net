namespace EdgeDB.Codecs
{
    internal class Integer16 : IScalarCodec<short>
    {
        public short Deserialize(PacketReader reader)
        {
            return reader.ReadInt16();
        }

        public void Serialize(PacketWriter writer, short value)
        {
            writer.Write(value);
        }
    }
}
