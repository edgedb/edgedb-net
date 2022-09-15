namespace EdgeDB.Binary.Codecs
{
    internal class Bytes : IScalarCodec<byte[]>
    {
        public byte[] Deserialize(ref PacketReader reader)
        {
            return reader.ConsumeByteArray();
        }

        public void Serialize(ref PacketWriter writer, byte[]? value)
        {
            if (value is not null)
                writer.Write(value);
        }
    }
}
