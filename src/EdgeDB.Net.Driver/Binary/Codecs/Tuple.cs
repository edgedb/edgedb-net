using EdgeDB.Binary;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Binary.Codecs
{
    internal class Tuple : ICodec<TransientTuple>
    {
        private readonly ICodec[] _innerCodecs;
        
        public Tuple(ICodec[] innerCodecs)
        {
            _innerCodecs = innerCodecs;
        }

        public TransientTuple Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if(_innerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {_innerCodecs.Length} codecs");
            }

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

            return new TransientTuple(_innerCodecs.Select(x => x.ConverterType).ToArray(), values);
        }

        public void Serialize(ref PacketWriter writer, TransientTuple value)
        {
            throw new NotSupportedException("Tuples cannot be passed in query arguments");
        }
    }
}
