using System.Runtime.CompilerServices;

namespace EdgeDB.Codecs
{
    internal class Tuple : ICodec<ITuple>
    {
        private readonly ICodec[] _innerCodecs;
        
        public Tuple(ICodec[] innerCodecs)
        {
            _innerCodecs = innerCodecs;
        }

        public ITuple Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if(_innerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {_innerCodecs.Length} codecs");
            }

            // create tuple dynamically
            var tupleType = Type.GetType($"System.Tuple`{numElements}");

            // TODO: support extended tuple types.
            if(tupleType is null)
            {
                throw new NotSupportedException("Failed to find tuple match.");
            }

            var constructedTupleType = tupleType.MakeGenericType(_innerCodecs.Select(x => x.ConverterType!).ToArray());

            // deserialize our values
            object?[] values = new object?[numElements];

            for(int i = 0; i != numElements; i++)
            {
                // skip reserved 
                reader.Skip(4);

                var length = reader.ReadInt32();
                
                if(length is 0)
                {
                    values[i] = null;
                    continue;
                }


                reader.ReadBytes(length, out var data);

                var innerReader = new PacketReader(data);
                values[i] = _innerCodecs[i].Deserialize(ref innerReader);
            }

            // construct our tuple
            return (ITuple)Activator.CreateInstance(constructedTupleType, values)!;
        }

        public void Serialize(PacketWriter writer, ITuple? value)
        {
            throw new NotSupportedException("Tuples cannot be passed in query arguments");
        }
    }
}
