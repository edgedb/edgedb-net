using EdgeDB.Binary;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TupleCodec
        : BaseCodec<TransientTuple>, IMultiWrappingCodec, ICacheableCodec
    {
        internal ICodec[] InnerCodecs;
        
        public TupleCodec(ICodec[] innerCodecs)
        {
            InnerCodecs = innerCodecs;
        }
        
        public override TransientTuple Deserialize(ref PacketReader reader)
        {
            var numElements = reader.ReadInt32();

            if(InnerCodecs.Length != numElements)
            {
                throw new ArgumentException($"codecs mismatch for tuple: expected {numElements} codecs, got {InnerCodecs.Length} codecs");
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
                values[i] = InnerCodecs[i].Deserialize(ref innerReader);
            }

            return new TransientTuple(InnerCodecs.Select(x => x.ConverterType).ToArray(), values);
        }

        public override void Serialize(ref PacketWriter writer, TransientTuple value)
        {
            throw new NotSupportedException("Tuples cannot be passed in query arguments");
        }

        public override string ToString()
        {
            return $"TupleCodec<{string.Join(", ", this.InnerCodecs.Select(x => x.ToString()))}>";
        }

        ICodec[] IMultiWrappingCodec.InnerCodecs
        {
            get => InnerCodecs;
            set
            {
                InnerCodecs = value;
            }
        }
    }
}
