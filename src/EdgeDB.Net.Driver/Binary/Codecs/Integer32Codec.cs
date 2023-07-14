namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer32Codec
        : BaseScalarCodec<int>
    {
        public override int Deserialize(ref PacketReader reader, CodecContext context)
        {
            return reader.ReadInt32();
        }

        public override void Serialize(ref PacketWriter writer, int value, CodecContext context)
        {
            writer.Write(value);
        }
    }
}
