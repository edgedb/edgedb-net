namespace EdgeDB.Binary.Codecs
{
    internal sealed class Bytes : BaseScalarCodec<byte[]>
    {
        public override byte[] Deserialize(ref PacketReader reader)
        {
            return reader.ConsumeByteArray();
        }

        public override void Serialize(ref PacketWriter writer, byte[]? value)
        {
            if (value is not null)
                writer.Write(value);
        }
    }
}
