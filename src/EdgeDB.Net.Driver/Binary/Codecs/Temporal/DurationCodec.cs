using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class DurationCodec : BaseTemporalCodec<Duration>
{
    public new static Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000112");

    public DurationCodec(CodecMetadata? metadata = null)
        : base(in Id, metadata)
    {
        AddConverter(From, To);
    }

    public override Duration Deserialize(ref PacketReader reader, CodecContext context)
    {
        var microseconds = reader.ReadInt64();

        // skip days and months
        reader.Skip(sizeof(int) + sizeof(int));

        return new Duration(microseconds);
    }

    public override void Serialize(ref PacketWriter writer, Duration value, CodecContext context)
    {
        writer.Write(value.Microseconds);
        writer.Write(0); // days
        writer.Write(0); // months
    }

    private Duration From(ref TimeSpan value)
        => new(value);

    private TimeSpan To(ref Duration value)
        => value.TimeSpan;

    public override string ToString()
        => "std::duration";
}
