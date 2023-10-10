using EdgeDB.Binary.Codecs;
using EdgeDB.Binary.Protocol.V1._0;
using EdgeDB.Binary.Protocol.V2._0.Descriptors;

namespace EdgeDB.Binary.Protocol.V2._0;

internal class V2ProtocolProvider : V1ProtocolProvider
{
    public V2ProtocolProvider(EdgeDBBinaryClient client)
        : base(client)
    {
    }

    public override ProtocolVersion Version { get; } = (2, 0);

    public override ITypeDescriptor GetDescriptor(ref PacketReader reader)
    {
        var length = reader.ReadUInt32();

        reader.Limit = (int)length;

        var type = (DescriptorType)reader.ReadByte();

        try
        {
            if (type is DescriptorType.TypeAnnotationText)
            {
                return new TypeAnnotationTextDescriptor(ref reader);
            }

            var id = reader.ReadGuid();

            return type switch
            {
                DescriptorType.Array => new ArrayTypeDescriptor(ref reader, in id),
                DescriptorType.Compound => new CompoundTypeDescriptor(ref reader, in id),
                DescriptorType.Enumeration => new EnumerationTypeDescriptor(ref reader, in id),
                DescriptorType.Input => new InputShapeDescriptor(ref reader, in id),
                DescriptorType.NamedTuple => new NamedTupleTypeDescriptor(ref reader, in id),
                DescriptorType.Object => new ObjectTypeDescriptor(ref reader, in id),
                DescriptorType.ObjectOutput => new ObjectOutputShapeDescriptor(ref reader, in id),
                DescriptorType.Range => new RangeTypeDescriptor(ref reader, in id),
                DescriptorType.Scalar => new ScalarTypeDescriptor(ref reader, in id),
                DescriptorType.Set => new SetDescriptor(ref reader, in id),
                DescriptorType.Tuple => new TupleTypeDescriptor(ref reader, in id),
                DescriptorType.MultiRange => new MultiRangeDescriptor(ref reader, in id),
                _ => throw new InvalidDataException($"No descriptor found for type {type}")
            };
        }
        finally
        {
            reader.Limit = -1;
        }
    }

    public override ICodec? BuildCodec<T>(
        in T descriptor,
        RelativeCodecDelegate getRelativeCodec,
        RelativeDescriptorDelegate getRelativeDescriptor)
    {
        var metadata = descriptor is IMetadataDescriptor metadataDescriptor
            ? metadataDescriptor.GetMetadata(getRelativeCodec, getRelativeDescriptor)
            : null;

        switch (descriptor)
        {
            case ArrayTypeDescriptor array:
            {
                ref var innerCodec = ref getRelativeCodec(array.Type)!;

                return new CompilableWrappingCodec(in array.Id, innerCodec, typeof(ArrayCodec<>), metadata);
            }
            case CompoundTypeDescriptor compound:
            {
                var codecs = new ICodec[compound.Components.Length];

                for (var i = 0; i != compound.Components.Length; i++)
                {
                    codecs[i] = getRelativeCodec(compound.Components[i])!;
                }

                return new CompoundCodec(in compound.Id, compound.Operation, codecs, metadata);
            }
            case EnumerationTypeDescriptor enumeration:
            {
                return new EnumerationCodec(in enumeration.Id, enumeration.Members, metadata);
            }
            case InputShapeDescriptor input:
            {
                var names = new string[input.Elements.Length];
                var codecs = new ICodec[input.Elements.Length];

                for (var i = 0; i != input.Elements.Length; i++)
                {
                    names[i] = input.Elements[i].Name;
                    codecs[i] = getRelativeCodec(input.Elements[i].TypePos)!;
                }

                return new SparceObjectCodec(in input.Id, codecs, names, metadata);
            }
            case NamedTupleTypeDescriptor namedTuple:
            {
                var names = new string[namedTuple.Elements.Length];
                var innerCodecs = new ICodec[namedTuple.Elements.Length];

                for (var i = 0; i != namedTuple.Elements.Length; i++)
                {
                    names[i] = namedTuple.Elements[i].Name;
                    innerCodecs[i] = getRelativeCodec(namedTuple.Elements[i].TypePos)!;
                }

                return new ObjectCodec(in namedTuple.Id, innerCodecs, names, metadata);
            }
            case ObjectTypeDescriptor:
                return null;
            case ObjectOutputShapeDescriptor objectOutput:
            {
                if (!objectOutput.IsEphemeralFreeShape)
                {
                    ref var objectDescriptor = ref getRelativeDescriptor(objectOutput.Type);

                    if (objectDescriptor is not ObjectTypeDescriptor objectTypeDescriptor)
                        throw new InvalidOperationException(
                            $"Expected ObjectTypeDescriptor but got {objectDescriptor}");

                    metadata = objectTypeDescriptor.GetMetadata(getRelativeCodec, getRelativeDescriptor);
                }

                var names = new string[objectOutput.Elements.Length];
                var innerCodecs = new ICodec[objectOutput.Elements.Length];

                for (var i = 0; i != objectOutput.Elements.Length; i++)
                {
                    names[i] = objectOutput.Elements[i].Name;
                    innerCodecs[i] = getRelativeCodec(objectOutput.Elements[i].TypePos)!;
                }

                return new ObjectCodec(in objectOutput.Id, innerCodecs, names, metadata);
            }
            case RangeTypeDescriptor range:
            {
                ref var innerCodec = ref getRelativeCodec(range.Type)!;

                return new CompilableWrappingCodec(in range.Id, innerCodec, typeof(RangeCodec<>), metadata);
            }
            case MultiRangeDescriptor multirange:
            {
                ref var innerCodec = ref getRelativeCodec(multirange.Type)!;

                return new CompilableWrappingCodec(in multirange.Id, innerCodec, typeof(MultiRangeCodec<>), metadata);
            }
            case ScalarTypeDescriptor scalar:
                throw new MissingCodecException(
                    $"Could not find the scalar type {scalar.Id}. Please file a bug report with your query that caused this error.");
            case SetDescriptor set:
            {
                ref var innerCodec = ref getRelativeCodec(set.Type)!;

                return new CompilableWrappingCodec(in set.Id, innerCodec, typeof(SetCodec<>), metadata);
            }
            case TupleTypeDescriptor tuple:
            {
                var innerCodecs = new ICodec[tuple.Elements.Length];
                for (var i = 0; i != tuple.Elements.Length; i++)
                    innerCodecs[i] = getRelativeCodec(tuple.Elements[i])!;

                return new TupleCodec(in tuple.Id, innerCodecs, metadata);
            }
            default:
                throw new MissingCodecException(
                    $"Could not find a type descriptor with type {descriptor.Id}. Please file a bug report with your query that caused this error.");
        }
    }
}
