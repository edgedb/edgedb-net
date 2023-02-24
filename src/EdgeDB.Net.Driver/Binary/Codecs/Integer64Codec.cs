namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer64Codec
        : BaseScalarCodec<long>
    {
        public override long Deserialize(ref PacketReader reader)
        {
            return reader.ReadInt64();
        }

        public override void Serialize(ref PacketWriter writer, long value)
        {
            writer.Write(value);
        }
    }
}
