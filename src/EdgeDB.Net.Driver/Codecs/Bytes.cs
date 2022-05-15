namespace EdgeDB.Codecs
{
    internal class Bytes : IScalarCodec<byte[]>
    {
        public byte[] Deserialize(PacketReader reader)
        {
            return reader.ConsumeByteArray();
        }

        public void Serialize(PacketWriter writer, byte[]? value)
        {
            if (value != null)
                writer.Write(value);
        }
    }
}
