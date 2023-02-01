namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer16Codec
        : BaseScalarCodec<short>
    {
        public override short Deserialize(ref PacketReader reader)
        {
            return reader.ReadInt16();
        }

        public override void Serialize(ref PacketWriter writer, short value)
        {
            writer.Write(value);
        }
    }
}
