namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float32 : BaseScalarCodec<float>
    {
        public override float Deserialize(ref PacketReader reader)
        {
            return reader.ReadSingle();
        }

        public override void Serialize(ref PacketWriter writer, float value)
        {
            writer.Write(value);
        }
    }
}
