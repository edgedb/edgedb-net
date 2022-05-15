namespace EdgeDB.Codecs
{
    internal class Memory : IScalarCodec<DataTypes.Memory>
    {
        public DataTypes.Memory Deserialize(PacketReader reader)
        {
            return new DataTypes.Memory(reader.ReadInt64());
        }

        public void Serialize(PacketWriter writer, DataTypes.Memory value)
        {
            writer.Write(value.TotalBytes);
        }
    }
}
