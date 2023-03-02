namespace EdgeDB.Binary.Codecs
{
    internal sealed class MemoryCodec
        : BaseScalarCodec<DataTypes.Memory>
    {
        public override DataTypes.Memory Deserialize(ref PacketReader reader, CodecContext context)
        {
            return new DataTypes.Memory(reader.ReadInt64());
        }

        public override void Serialize(ref PacketWriter writer, DataTypes.Memory value, CodecContext context)
        {
            writer.Write(value.TotalBytes);
        }
    }
}
