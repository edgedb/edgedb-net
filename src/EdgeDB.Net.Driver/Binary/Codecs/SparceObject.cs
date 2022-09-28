using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class SparceObject : ICodec<object>
    {
        private readonly ICodec[] _innerCodecs;
        private readonly string[] _fieldNames;

        internal SparceObject(InputShapeDescriptor descriptor, List<ICodec> codecs)
        {
            _innerCodecs = new ICodec[descriptor.Shapes.Length];
            _fieldNames = new string[descriptor.Shapes.Length];

            for (int i = 0; i != descriptor.Shapes.Length; i++)
            {
                var shape = descriptor.Shapes[i];
                _innerCodecs[i] = codecs[shape.TypePos];
                _fieldNames[i] = shape.Name;
            }
        }

        public object? Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if (_innerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {_innerCodecs.Length} codecs");
            }

            dynamic data = new ExpandoObject();
            var dataDictionary = (IDictionary<string, object?>)data;

            for (int i = 0; i != numElements; i++)
            {
                var index = reader.ReadInt32();
                var elementName = _fieldNames[index];

                var length = reader.ReadInt32();

                if (length is -1)
                {
                    dataDictionary.Add(elementName, null);
                    continue;
                }

                reader.ReadBytes(length, out var innerData);

                object? value;

                value = _innerCodecs[i].Deserialize(innerData);

                dataDictionary.Add(elementName, value);
            }

            return data;
        }

        public void Serialize(ref PacketWriter writer, object? value)
        {
            if (value is not IDictionary<string, object?> dict)
                throw new InvalidOperationException($"Cannot serialize {value?.GetType() ?? Type.Missing} as a sparce object.");
            
            if(!dict.Any())
            {
                writer.Write(0);
                return;
            }

            writer.Write(dict.Count);

            foreach(var element in dict)
            {
                var index = Array.IndexOf(_fieldNames, element.Key);

                if (index == -1)
                    throw new MissingCodecException($"No serializer found for field {element.Key}");

                writer.Write(index);

                if (element.Value == null)
                    writer.Write(-1);
                else
                {
                    var codec = _innerCodecs[index];
                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => codec.Serialize(ref innerWriter, element.Value));
                }
            }
        }
    }
}
