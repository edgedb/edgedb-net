namespace EdgeDB.Binary.Codecs
{
    internal class Memory : IScalarCodec<DataTypes.Memory>
    {
        public DataTypes.Memory Deserialize(ref PacketReader reader)
        {
            return new DataTypes.Memory(reader.ReadInt64());
        }

        public void Serialize(ref PacketWriter writer, DataTypes.Memory value)
        {
            writer.Write(value.TotalBytes);
        }
    }
}
