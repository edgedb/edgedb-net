using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.Utils.FSharp;
using System.Collections.Concurrent;

namespace EdgeDB.Binary.Codecs;

internal sealed class TypeInitializedObjectCodec : ObjectCodec
{
    private readonly EdgeDBTypeDeserializeInfo _deserializer;

    public TypeInitializedObjectCodec(Type target, ObjectCodec codec)
        : base(codec.Id, codec.InnerCodecs, codec.PropertyNames, codec.Metadata)
    {
        if (!TypeBuilder.TryGetTypeDeserializerInfo(target, out _deserializer!))
            throw new NoTypeConverterException($"Failed to find type deserializer for {target}");

        TargetType = target;
        Parent = codec;
    }

    public EdgeDBTypeDeserializeInfo Deserializer
        => _deserializer;

    public ObjectCodec Parent { get; }

    public Type TargetType { get; }

    public override object? Deserialize(ref PacketReader reader, CodecContext context)
    {
        // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
        // so we need to pass the underlying data as a reference and wrap a new reader ontop.
        // This method ensures we're not copying the packet in memory again but the downside is
        // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
        var enumerator = new ObjectEnumerator(
            in reader.Data,
            reader.Position,
            PropertyNames,
            InnerCodecs,
            context
        );

        try
        {
            return _deserializer.Factory(ref enumerator);
        }
        catch (Exception x)
        {
            throw new EdgeDBException($"Failed to deserialize object to {TargetType}", x);
        }
        finally
        {
            // set the readers position to the enumerators' readers position.
            reader.Position = enumerator.Reader.Position;
        }
    }
}

internal class ObjectCodec
    : BaseArgumentCodec<object>, IMultiWrappingCodec, ICacheableCodec
{
    public readonly string[] PropertyNames;

    private ConcurrentDictionary<Type, TypeInitializedObjectCodec>? _typeCodecs;
    public ICodec[] InnerCodecs;

    internal ObjectCodec(in Guid id, ICodec[] innerCodecs, string[] propertyNames, CodecMetadata? metadata = null)
        : base(in id, metadata)
    {
        InnerCodecs = innerCodecs;
        PropertyNames = propertyNames;
    }

    ICodec[] IMultiWrappingCodec.InnerCodecs
    {
        get => InnerCodecs;
        set => InnerCodecs = value;
    }

    public TypeInitializedObjectCodec GetOrCreateTypeCodec(Type type)
        => (_typeCodecs ??= new ConcurrentDictionary<Type, TypeInitializedObjectCodec>()).GetOrAdd(type,
            t => new TypeInitializedObjectCodec(t, this));

    public override object? Deserialize(ref PacketReader reader, CodecContext context)
    {
        // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
        // so we need to pass the underlying data as a reference and wrap a new reader ontop.
        // This method ensures we're not copying the packet in memory again but the downside is
        // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
        var enumerator = new ObjectEnumerator(
            in reader.Data,
            reader.Position,
            PropertyNames,
            InnerCodecs,
            context
        );

        try
        {
            return enumerator.ToDynamic();
        }
        catch (Exception x)
        {
            throw new EdgeDBException("Failed to deserialize object", x);
        }
        finally
        {
            // set the readers position to the enumerators' readers position.
            reader.Position = enumerator.Reader.Position;
        }
    }

    public override void SerializeArguments(ref PacketWriter writer, object? value, CodecContext context)
        => Serialize(ref writer, value, context);

    public override void Serialize(ref PacketWriter writer, object? value, CodecContext context)
    {
        object?[]? values = null;

        if (value is IDictionary<string, object?> dict)
            values = PropertyNames.Select(x => dict[x]).ToArray();
        else if (value is object?[] arr)
            value = arr;

        if (values is null)
        {
            throw new ArgumentException($"Expected dynamic object or array but got {value?.GetType()?.Name ?? "null"}");
        }

        writer.Write(values.Length);

        // TODO: maybe cache the visited codecs based on the 'value'.
        var visitor = context.CreateTypeVisitor();

        for (var i = 0; i != values.Length; i++)
        {
            var element = values[i];

            // reserved
            writer.Write(0);

            if (FSharpOptionInterop.TryGet(element, out var option))
            {
                if (!option.HasValue)
                {
                    writer.Write(-1);
                    continue;
                }

                element = option.Value;
            }

            // encode
            if (element is null)
            {
                writer.Write(-1);
            }
            else
            {
                var innerCodec = InnerCodecs[i];

                // special case for enums
                if (element.GetType().IsEnum && innerCodec is TextCodec)
                    element = element.ToString();
                else
                {
                    visitor.SetTargetType(element.GetType());
                    visitor.Visit(ref innerCodec);
                    visitor.Reset();
                }

                writer.WriteToWithInt32Length((ref PacketWriter innerWriter) =>
                    innerCodec.Serialize(ref innerWriter, element, context));
            }
        }
    }

    public override string ToString()
        => "object";
}
