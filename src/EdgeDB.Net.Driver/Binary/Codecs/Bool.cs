namespace EdgeDB.Binary.Codecs
{
    internal sealed class Bool : IScalarCodec<bool>
    {
        public bool Deserialize(ref PacketReader reader)
        {
            return reader.ReadBoolean();
        }

        public void Serialize(ref PacketWriter writer, bool value)
        {
            writer.Write(value);
        }
    }
}
