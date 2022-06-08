using System.Dynamic;

namespace EdgeDB.Codecs
{
    internal class Object : ICodec<object>, IArgumentCodec<object>
    {
        private readonly ICodec[] _innerCodecs;
        private readonly string[] _propertyNames;

        internal Object(ICodec[] innerCodecs, string[] propertyNames)
        {
            _innerCodecs = innerCodecs;
            _propertyNames = propertyNames;
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
                // reserved
                reader.Skip(4);
                var name = _propertyNames[i];
                var length = reader.ReadInt32();

                if(length is -1)
                {
                    dataDictionary.Add(name, null);
                    continue;
                }

                reader.ReadBytes(length, out var innerData);

                object? value;

                value = _innerCodecs[i].Deserialize(innerData);

                dataDictionary.Add(name, value);
            }

            return data;
        }

        public void Serialize(PacketWriter writer, object? value)
        {
            throw new NotImplementedException();
        }

        public void SerializeArguments(PacketWriter writer, object? value)
        {
            object?[]? values = null;

            if (value is IDictionary<string, object?> dict)
                values = dict.Values.ToArray();
            else if (value is object?[] arr)
                value = arr;

            if (values is null)
            {
                throw new ArgumentException($"Expected dynamic object or array but got {value?.GetType()?.Name ?? "null"}");
            }

            using var innerWriter = new PacketWriter();
            for (int i = 0; i != values.Length; i++)
            {
                var element = values[i];
                var innerCodec = _innerCodecs[i];

                // reserved
                innerWriter.Write(0);

                // encode
                if (element is null)
                {
                    innerWriter.Write(-1);
                }
                else
                {
                    // special case for enums
                    if (element.GetType().IsEnum && innerCodec is Text)
                        element = element.ToString();

                    var elementBuff = innerCodec.Serialize(element);

                    innerWriter.Write(elementBuff.Length);
                    innerWriter.Write(elementBuff);
                }
            }

            writer.Write((int)innerWriter.BaseStream.Length + 4);
            writer.Write(values.Length);
            writer.Write(innerWriter);
        }
    }
}
