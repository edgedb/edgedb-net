using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class DateDurationCodec : BaseTemporalCodec<DateDuration>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public DateDurationCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(From, To);
    }

    public override DateDuration Deserialize(ref PacketReader reader, CodecContext context)
    {
        reader.Skip(sizeof(long));
        var days = reader.ReadInt32();
        var months = reader.ReadInt32();

        return new DateDuration(days, months);
    }

    public override void Serialize(ref PacketWriter writer, DateDuration value, CodecContext context)
    {
        writer.Write(0L);
        writer.Write(value.Days);
        writer.Write(value.Months);
    }

    private DateDuration From(ref TimeSpan value)
        => new(value);

    private TimeSpan To(ref DateDuration value)
        => value.TimeSpan;

    public override string ToString()
        => "cal::date_duration";
}
