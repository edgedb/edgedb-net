using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class MemoryCodec
        : BaseScalarCodec<DataTypes.Memory>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000130");

        public MemoryCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

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
