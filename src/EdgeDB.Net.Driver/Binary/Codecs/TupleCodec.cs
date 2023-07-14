using EdgeDB.Binary;
using EdgeDB.DataTypes;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TupleCodec
        : BaseComplexCodec<TransientTuple>, IMultiWrappingCodec, ICacheableCodec
    {
        internal ICodec[] InnerCodecs;
        
        public TupleCodec(ICodec[] innerCodecs)
        {
            InnerCodecs = innerCodecs;

            AddConverter(TransientTuple.IsReferenceTupleType, From, ToReferenceTuple);
            AddConverter(TransientTuple.IsValueTupleType, From, ToValueTuple);
        }

        private TransientTuple From(ref ITuple? tuple)
            => tuple is null ? default : new(tuple);

        private ITuple ToValueTuple(ref TransientTuple tuple)
            => tuple.ToValueTuple();

        private ITuple ToReferenceTuple(ref TransientTuple tuple)
            => tuple.ToReferenceTuple();

        public override TransientTuple Deserialize(ref PacketReader reader, CodecContext context)
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
                values[i] = InnerCodecs[i].Deserialize(ref innerReader, context);
            }

            return new TransientTuple(InnerCodecs.Select(x => x.ConverterType).ToArray(), values);
        }

        public override void Serialize(ref PacketWriter writer, TransientTuple value, CodecContext context)
        {
            if(value.Length == 0)
            {
                writer.Write(-1);
                return;
            }

            if (InnerCodecs.Length != value.Length)
                throw new ArgumentException("Tuple length does not match descriptor length");

            writer.Write(value.Length);

            for(int i = 0; i != value.Length; i++)
            {
                var codec = InnerCodecs[i];
                var elementValue = value.RawValues[i];

                writer.Write(0); // reserved

                if (elementValue is null)
                    writer.Write(-1);
                else
                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => codec.Serialize(ref innerWriter, elementValue, context));
            }
        }

        public override string ToString()
        {
            return $"TupleCodec<{string.Join(", ", this.InnerCodecs.Select(x => x.ToString()))}>";
        }

        public Type CreateValueTupleType()
        {
            var typeArr = InnerCodecs.Select(x => x.ConverterType).ToArray();
            return CreateTupleTypePart(typeArr);
        }

        private Type CreateTupleTypePart(Type[] elements, int offset = 0)
        {
            Type tupleType;
            if (InnerCodecs.Length - offset > 7)
            {
                var innerTuple = CreateTupleTypePart(elements, offset + 7);

                var types = new Type[8];
                types[7] = innerTuple;
                elements[offset..(offset + 7)].CopyTo(types, 0);

                tupleType = TransientTuple.ValueTupleTypeMap[types.Length - 1];
                return tupleType.MakeGenericType(types);
            }

            var remainingElements = elements[offset..];

            tupleType = TransientTuple.ValueTupleTypeMap[remainingElements.Length - 1];
            return tupleType.MakeGenericType(remainingElements);
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
