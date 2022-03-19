using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    internal class Tuple : ICodec<ITuple>
    {
        private readonly ICodec[] _innerCodecs;
        
        public Tuple(ICodec[] innerCodecs)
        {
            _innerCodecs = innerCodecs;
        }

        public ITuple Deserialize(PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if(_innerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {_innerCodecs.Length} codecs");
            }

            // create tuple dynamically
            var tupleType = Type.GetType($"System.Tuple`{numElements}");

            // TODO: support extended tuple types.
            if(tupleType == null)
            {
                throw new NotSupportedException("Failed to find tuple match.");
            }

            var constructedTupleType = tupleType.MakeGenericType(_innerCodecs.Select(x => x.ConverterType!).ToArray());

            // deserialize our values
            object?[] values = new object?[numElements];

            for(int i = 0; i != numElements; i++)
            {
                // skip reserved 
                reader.ReadInt32();

                var length = reader.ReadInt32();
                
                if(length == 0)
                {
                    values[i] = null;
                    continue;
                }


                var data = reader.ReadBytes(length);

                // TODO: optimize this?
                using(var innerReader = new PacketReader(data))
                {
                    values[i] = _innerCodecs[i].Deserialize(innerReader);
                }
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
