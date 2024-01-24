using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.DataTypes;
using SysRange = System.Range;

namespace EdgeDB.Binary.Codecs;

internal sealed class RangeCodec<T>
    : BaseComplexCodec<Range<T>>, IWrappingCodec, ICacheableCodec, ICompiledCodec
    where T : struct
{
    [Flags]
    public enum RangeFlags : byte
    {
        Empty = 1 << 0,
        IncludeLowerBound = 1 << 1,
        IncludeUpperBound = 1 << 2,
        InfiniteLowerBound = 1 << 3,
        InfiniteUpperBound = 1 << 4
    }

    public Type CompiledFrom { get; }
    public CompilableWrappingCodec Template { get; }

    private ICodec<T> _innerCodec;

    public RangeCodec(
        in Guid id,
        Type compiledFrom,
        CompilableWrappingCodec template,
        ICodec<T> innerCodec,
        CodecMetadata? metadata = null)
        : base(in id, metadata)
    {
        _innerCodec = innerCodec;

        CompiledFrom = compiledFrom;
        Template = template;

        AddConverter(From, To);
    }

    ICodec IWrappingCodec.InnerCodec
    {
        get => _innerCodec;
        set
        {
            if (value is null)
                throw new NullReferenceException("Attempted to supply a 'null' instance codec to a wrapping codec");

            if (value is not ICodec<T> correctedValue)
                throw new NotSupportedException($"Cannot set {value} as a Codec<T>");

            _innerCodec = correctedValue;
        }
    }

    private static Range<T> From(ref SysRange transient)
    {
        EnsureCanUseSysRange();

        if (transient.Start.Value is not T start || transient.End.Value is not T end)
            throw new NotSupportedException($"Cannot use system range as its index type is not {typeof(T)}");

        return new Range<T>(start, end);
    }

    private static SysRange To(ref Range<T> range)
    {
        EnsureCanUseSysRange();

        int start = 0, end = 0;

        if (range.Lower.HasValue)
        {
            if (range.Lower.Value is not int v)
                throw new NotSupportedException("Cannot use edgedb range as its inner type is not int32");

            start = v;
        }

        if (range.Upper.HasValue)
        {
            if (range.Upper.Value is not int v)
                throw new NotSupportedException("Cannot use edgedb range as its inner type is not int32");

            end = v;
        }

        return new SysRange(start, end);
    }

    private static void EnsureCanUseSysRange()
    {
        // only support int32
        if (typeof(T) != typeof(int))
            throw new NotSupportedException(
                "EdgeDB.DataTypes.Range<T> must be of int32 to implicitly convert to/from System.Range");
    }

    public override Range<T> Deserialize(ref PacketReader reader, CodecContext context)
    {
        var flags = (RangeFlags)reader.ReadByte();

        if ((flags & RangeFlags.Empty) != 0)
            return Range<T>.Empty();

        T? lowerBound = null, upperBound = null;


        if ((flags & RangeFlags.InfiniteLowerBound) == 0)
        {
            reader.Skip(4);
            lowerBound = _innerCodec.Deserialize(ref reader, context);
        }

        if ((flags & RangeFlags.IncludeUpperBound) == 0)
        {
            reader.Skip(4);
            upperBound = _innerCodec.Deserialize(ref reader, context);
        }

        return new Range<T>(lowerBound, upperBound, (flags & RangeFlags.IncludeLowerBound) != 0,
            (flags & RangeFlags.IncludeUpperBound) != 0);
    }

    public override void Serialize(ref PacketWriter writer, Range<T> value, CodecContext context)
    {
        var flags = value.IsEmpty
            ? RangeFlags.Empty
            : (value.IncludeLower ? RangeFlags.IncludeLowerBound : 0) |
              (value.IncludeUpper ? RangeFlags.IncludeUpperBound : 0) |
              (!value.Lower.HasValue ? RangeFlags.InfiniteLowerBound : 0) |
              (!value.Upper.HasValue ? RangeFlags.InfiniteUpperBound : 0);

        writer.Write((byte)flags);

        if (value.Lower.HasValue)
        {
            writer.WriteToWithInt32Length((ref PacketWriter innerWriter) =>
                _innerCodec.Serialize(ref innerWriter, value.Lower.Value, context));
        }

        if (value.Upper.HasValue)
        {
            writer.WriteToWithInt32Length((ref PacketWriter innerWriter) =>
                _innerCodec.Serialize(ref innerWriter, value.Upper.Value, context));
        }
    }

    public override string ToString()
        => "range";
}
