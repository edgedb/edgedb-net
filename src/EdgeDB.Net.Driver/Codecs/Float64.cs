namespace EdgeDB.Codecs
{
    internal class Float64 : IScalarCodec<double>
    {
        public double Deserialize(ref PacketReader reader)
        {
            return reader.ReadDouble();
        }

        public void Serialize(PacketWriter writer, double value)
        {
            writer.Write(value);
        }
    }
}
