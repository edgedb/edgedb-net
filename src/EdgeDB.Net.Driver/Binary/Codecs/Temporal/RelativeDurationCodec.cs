using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class RelativeDurationCodec : BaseTemporalCodec<RelativeDuration>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public RelativeDurationCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(From, To);
    }

    public override RelativeDuration Deserialize(ref PacketReader reader, CodecContext context)
    {
        var microseconds = reader.ReadInt64();
        var days = reader.ReadInt32();
        var months = reader.ReadInt32();

        return new RelativeDuration(microseconds, days, months);
    }

    public override void Serialize(ref PacketWriter writer, RelativeDuration value, CodecContext context)
    {
        writer.Write(value.Microseconds);
        writer.Write(value.Days);
        writer.Write(value.Months);
    }

    private RelativeDuration From(ref TimeSpan value)
        => new(value);

    private TimeSpan To(ref RelativeDuration value)
        => value.TimeSpan;

    public override string ToString()
        => "cal::relative_duration";
}
