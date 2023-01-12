namespace EdgeDB.Binary.Codecs
{
    internal sealed class Bool : BaseScalarCodec<bool>
    {
        public override bool Deserialize(ref PacketReader reader)
        {
            return reader.ReadBoolean();
        }

        public override void Serialize(ref PacketWriter writer, bool value)
        {
            writer.Write(value);
        }
    }
}
