namespace EdgeDB.Binary.Codecs
{
    internal sealed class Integer32 : BaseScalarCodec<int>
    {
        public override int Deserialize(ref PacketReader reader)
        {
            return reader.ReadInt32();
        }

        public override void Serialize(ref PacketWriter writer, int value)
        {
            writer.Write(value);
        }
    }
}
