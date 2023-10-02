using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class LocalDateCodec : BaseTemporalCodec<LocalDate>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public LocalDateCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(From, To);
    }

    public override LocalDate Deserialize(ref PacketReader reader, CodecContext context)
    {
        var days = reader.ReadInt32();

        return new LocalDate(days);
    }

    public override void Serialize(ref PacketWriter writer, LocalDate value, CodecContext context) =>
        writer.Write(value.Days);

    private LocalDate From(ref DateOnly value)
        => new(value);

    private DateOnly To(ref LocalDate value)
        => value.DateOnly;

    public override string ToString()
        => "cal::local_date";
}
