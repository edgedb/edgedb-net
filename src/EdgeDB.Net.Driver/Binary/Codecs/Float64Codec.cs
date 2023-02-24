namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float64Codec
        : BaseScalarCodec<double>
    {
        public override double Deserialize(ref PacketReader reader)
        {
            return reader.ReadDouble();
        }

        public override void Serialize(ref PacketWriter writer, double value)
        {
            writer.Write(value);
        }
    }
}
