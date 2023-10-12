using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;
using DateTime = System.DateTime;

namespace EdgeDB.Binary.Codecs;

internal sealed class LocalDateTimeCodec : BaseTemporalCodec<LocalDateTime>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public LocalDateTimeCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(FromDT, ToDT);
        AddConverter(FromDTO, ToDTO);
    }

    public override LocalDateTime Deserialize(ref PacketReader reader, CodecContext context)
    {
        var microseconds = reader.ReadInt64();

        return new LocalDateTime(microseconds);
    }

    public override void Serialize(ref PacketWriter writer, LocalDateTime value, CodecContext context) =>
        writer.Write(value.Microseconds);

    private LocalDateTime FromDT(ref DateTime value)
        => new(value);

    private DateTime ToDT(ref LocalDateTime value)
        => value.DateTime;

    private LocalDateTime FromDTO(ref DateTimeOffset value)
        => new(value);

    private DateTimeOffset ToDTO(ref LocalDateTime value)
        => value.DateTimeOffset;

    public override string ToString()
        => "cal::local_datetime";
}
