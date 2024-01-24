using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;
using EdgeDB.Models.DataTypes;

namespace EdgeDB.Binary.Codecs;

internal sealed class MultiRangeCodec<T>
    : BaseCodec<MultiRange<T>>, IWrappingCodec, ICacheableCodec, ICompiledCodec
    where T : struct
{
    public Type CompiledFrom { get; }
    public CompilableWrappingCodec Template { get; }

    private RangeCodec<T> _rangeCodec;

    public MultiRangeCodec(
        in Guid id,
        Type compiledFrom,
        CompilableWrappingCodec template,
        ICodec<T> rangeInnerCodec,
        CodecMetadata? metadata
        ) : base(in id, metadata)
    {
        CompiledFrom = compiledFrom;
        Template = template;
        _rangeCodec = new RangeCodec<T>(
            in id,
            compiledFrom,
            template,
            rangeInnerCodec,
            metadata
        );
    }

    public override void Serialize(ref PacketWriter writer, MultiRange<T> value, CodecContext context)
    {
        writer.Write(value.Length);

        for (int i = 0; i != value.Length; i++)
        {
            writer.WriteToWithInt32Length(
                (ref PacketWriter innerWriter) => _rangeCodec.Serialize(ref innerWriter, value[i], context));
        }
    }

    public override MultiRange<T> Deserialize(ref PacketReader reader, CodecContext context)
    {
        var length = reader.ReadInt32();

        var elements = new Range<T>[length];

        for (int i = 0; i != length; i++)
        {
            reader.Limit = reader.ReadInt32();
            elements[i] = _rangeCodec.Deserialize(ref reader, context);
            reader.Limit = -1;
        }

        return new MultiRange<T>(elements);
    }

    public ICodec InnerCodec
    {
        get => _rangeCodec;
        set
        {
            if (value is not RangeCodec<T> r)
                throw new ArgumentException($"Expected a range codec, but got {value}");

            _rangeCodec = r;
        }
    }

    public override string ToString()
        => "multirange";
}
