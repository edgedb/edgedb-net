using EdgeDB.Binary.Protocol.Common.Descriptors;
using DateTime = EdgeDB.DataTypes.DateTime;

namespace EdgeDB.Binary.Codecs;

internal sealed class DateTimeCodec : BaseTemporalCodec<DateTime>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public DateTimeCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(FromDT, ToDT);
        AddConverter(FromDTO, ToDTO);
    }

    public override DateTime Deserialize(ref PacketReader reader, CodecContext context)
    {
        var microseconds = reader.ReadInt64();

        return new DateTime(microseconds);
    }

    public override void Serialize(ref PacketWriter writer, DateTime value, CodecContext context) =>
        writer.Write(value.Microseconds);

    private DateTime FromDT(ref System.DateTime value)
        => new(value);

    private System.DateTime ToDT(ref DateTime value)
        => value.DateTimeOffset.DateTime;

    private DateTime FromDTO(ref DateTimeOffset value)
        => new(value);

    private DateTimeOffset ToDTO(ref DateTime value)
        => value.DateTimeOffset;

    public override string ToString()
        => "std::datetime";
}
