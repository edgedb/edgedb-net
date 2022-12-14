namespace EdgeDB.Binary.Codecs
{
    internal sealed class Float64 : IScalarCodec<double>
    {
        public double Deserialize(ref PacketReader reader)
        {
            return reader.ReadDouble();
        }

        public void Serialize(ref PacketWriter writer, double value)
        {
            writer.Write(value);
        }
    }
}
