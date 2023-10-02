using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class LocalTimeCodec : BaseTemporalCodec<LocalTime>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public LocalTimeCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(FromTS, ToTS);
        AddConverter(FromTO, ToTO);
    }

    public override LocalTime Deserialize(ref PacketReader reader, CodecContext context)
    {
        var microseconds = reader.ReadInt64();

        return new LocalTime(microseconds);
    }

    public override void Serialize(ref PacketWriter writer, LocalTime value, CodecContext context) =>
        writer.Write(value.Microseconds);

    private LocalTime FromTS(ref TimeSpan value)
        => new(value);

    private TimeSpan ToTS(ref LocalTime value)
        => value.TimeSpan;

    private LocalTime FromTO(ref TimeOnly value)
        => new(value);

    private TimeOnly ToTO(ref LocalTime value)
        => value.TimeOnly;

    public override string ToString()
        => "cal::local_time";
}
