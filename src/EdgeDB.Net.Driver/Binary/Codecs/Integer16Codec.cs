namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer16Codec
        : BaseScalarCodec<short>
    {
        public override short Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadInt16();
        }

        public override void Serialize(ref PacketWriter writer, short value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
