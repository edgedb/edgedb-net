using EdgeDB.Binary.Protocol.Common.Descriptors;
using System.Dynamic;

namespace EdgeDB.Binary.Codecs;

internal sealed class SparceObjectCodec
    : BaseCodec<object>, ICacheableCodec, IMultiWrappingCodec
{
    public readonly string[] FieldNames;
    public ICodec[] InnerCodecs;

    public SparceObjectCodec(in Guid id, ICodec[] innerCodecs, string[] fieldNames, CodecMetadata? metadata = null)
        : base(in id, metadata)
    {
        FieldNames = fieldNames;
        InnerCodecs = innerCodecs;
    }

    ICodec[] IMultiWrappingCodec.InnerCodecs
    {
        get => InnerCodecs;
        set => InnerCodecs = value;
    }

    public override object? Deserialize(ref PacketReader reader, CodecContext context)
    {
        var numElements = reader.ReadInt32();

        if (InnerCodecs.Length != numElements)
        {
            throw new ArgumentException(
                $"codecs mismatch for tuple: expected {numElements} codecs, got {InnerCodecs.Length} codecs");
        }

        dynamic data = new ExpandoObject();
        var dataDictionary = (IDictionary<string, object?>)data;

        for (var i = 0; i != numElements; i++)
        {
            var index = reader.ReadInt32();
            var elementName = FieldNames[index];

            var length = reader.ReadInt32();

            if (length is -1)
            {
                dataDictionary.Add(elementName, null);
                continue;
            }

            reader.ReadBytes(length, out var innerData);

            object? value;

            value = InnerCodecs[i].Deserialize(context, in innerData);

            dataDictionary.Add(elementName, value);
        }

        return data;
    }

    public override void Serialize(ref PacketWriter writer, object? value, CodecContext context)
    {
        if (value is not IDictionary<string, object?> dict)
            throw new InvalidOperationException(
                $"Cannot serialize {value?.GetType() ?? Type.Missing} as a sparce object.");

        if (!dict.Any())
        {
            writer.Write(0);
            return;
        }

        writer.Write(dict.Count);

        foreach (var element in dict)
        {
            var index = Array.IndexOf(FieldNames, element.Key);

            if (index == -1)
                throw new MissingCodecException($"No serializer found for field {element.Key}");

            writer.Write(index);

            if (element.Value == null)
                writer.Write(-1);
            else
            {
                var codec = InnerCodecs[index];

                writer.WriteToWithInt32Length((ref PacketWriter innerWriter) =>
                    codec.Serialize(ref innerWriter, element.Value, context));
            }
        }
    }

    public override string ToString()
        => "sparce_object";
}
