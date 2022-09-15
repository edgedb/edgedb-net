namespace EdgeDB.Binary.Codecs
{
    internal class Float32 : IScalarCodec<float>
    {
        public float Deserialize(ref PacketReader reader)
        {
            return reader.ReadSingle();
        }

        public void Serialize(ref PacketWriter writer, float value)
        {
            writer.Write(value);
        }
    }
}
