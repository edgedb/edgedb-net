namespace EdgeDB.Binary.Codecs
{
    internal sealed class BytesCodec
        : BaseScalarCodec<byte[]>
    {
        public override byte[] Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ConsumeByteArray();
        }

        public override void Serialize(ref PacketWriter writer, byte[]? value, CodecContext context)
        {
            if (value is not null)
                writer.Write(value);
        }
    }
}
