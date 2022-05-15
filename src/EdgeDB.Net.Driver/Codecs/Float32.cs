namespace EdgeDB.Codecs
{
    internal class Float32 : IScalarCodec<float>
    {
        public float Deserialize(PacketReader reader)
        {
            return reader.ReadSingle();
        }

        public void Serialize(PacketWriter writer, float value)
        {
            writer.Write(value);
        }
    }
}
