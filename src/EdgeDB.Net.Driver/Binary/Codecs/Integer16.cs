namespace EdgeDB.Binary.Codecs
{
    internal class Integer16 : IScalarCodec<short>
    {
        public short Deserialize(ref PacketReader reader)
        {
            return reader.ReadInt16();
        }

        public void Serialize(PacketWriter writer, short value)
        {
            writer.Write(value);
        }
    }
}
