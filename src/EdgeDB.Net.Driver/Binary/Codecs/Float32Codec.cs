namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float32Codec
        : BaseScalarCodec<float>
    {
        public override float Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadSingle();
        }

        public override void Serialize(ref PacketWriter writer, float value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
