namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float64Codec
        : BaseScalarCodec<double>
    {
        public override double Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadDouble();
        }

        public override void Serialize(ref PacketWriter writer, double value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
