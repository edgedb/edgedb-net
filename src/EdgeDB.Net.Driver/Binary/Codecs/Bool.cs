namespace EdgeDB.Binary.Codecs
{
    internal class Bool : IScalarCodec<bool>
    {
        public bool Deserialize(ref PacketReader reader)
        {
            return reader.ReadBoolean();
        }

        public void Serialize(PacketWriter writer, bool value)
        {
            writer.Write(value);
        }
    }
}
