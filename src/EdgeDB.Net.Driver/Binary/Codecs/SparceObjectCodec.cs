using EdgeDB.Binary;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class SparceObjectCodec
        : BaseCodec<object>, ICacheableCodec, IObjectCodec
    {
        public ICodec[] InnerCodecs;
        public readonly string[] FieldNames;

        internal SparceObjectCodec(InputShapeDescriptor descriptor, List<ICodec> codecs)
        {
            InnerCodecs = new ICodec[descriptor.Shapes.Length];
            FieldNames = new string[descriptor.Shapes.Length];

            for (int i = 0; i != descriptor.Shapes.Length; i++)
            {
                var shape = descriptor.Shapes[i];
                InnerCodecs[i] = codecs[shape.TypePos];
                FieldNames[i] = shape.Name;
            }
        }

        public override object? Deserialize(ref PacketReader reader, CodecContext context)
        {
            var numElements = reader.ReadInt32();

            if (InnerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {InnerCodecs.Length} codecs");
            }

            dynamic data = new ExpandoObject();
            var dataDictionary = (IDictionary<string, object?>)data;

            for (int i = 0; i != numElements; i++)
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

                value = InnerCodecs[i].Deserialize(context, innerData);

                dataDictionary.Add(elementName, value);
            }

            return data;
        }

        public override void Serialize(ref PacketWriter writer, object? value, CodecContext context)
        {
            if (value is not IDictionary<string, object?> dict)
                throw new InvalidOperationException($"Cannot serialize {value?.GetType() ?? Type.Missing} as a sparce object.");
            
            if(!dict.Any())
            {
                writer.Write(0);
                return;
            }

            writer.Write(dict.Count);

            var visitor = context.CreateTypeVisitor();

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

                    // ignore nested sparce object codecs, they will be walked
                    // in their serialize method.
                    visitor.SetTargetType(codec is SparceObjectCodec
                        ? typeof(void)
                        : element.Value.GetType()
                    );

                    visitor.Visit(ref codec);
                    visitor.Reset();

                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => codec.Serialize(ref innerWriter, element.Value, context));
                }
            }
        }

        public override string ToString()
        {
            return $"SparseObjectCodec<{string.Join(", ", InnerCodecs.Zip(FieldNames).Select(x => $"[{x.Second}: {x.First}]"))}>";
        }

        string[] IObjectCodec.PropertyNames => FieldNames;

        ICodec[] IObjectCodec.PropertyCodecs => InnerCodecs;
    }
}
