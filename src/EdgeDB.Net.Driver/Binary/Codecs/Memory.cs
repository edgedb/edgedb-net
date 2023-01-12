namespace EdgeDB.Binary.Codecs
{
    internal sealed class Memory : BaseScalarCodec<DataTypes.Memory>
    {
        public override DataTypes.Memory Deserialize(ref PacketReader reader)
        {
            return new DataTypes.Memory(reader.ReadInt64());
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Memory value)
        {
            writer.Write(value.TotalBytes);
        }
    }
}
