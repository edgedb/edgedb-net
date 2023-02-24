namespace EdgeDB.Binary.Codecs
{
    internal sealed class ArrayCodec<T>
        : BaseCodec<T?[]>, IWrappingCodec, ICacheableCodec
    {
        public static readonly byte[] EMPTY_ARRAY = new byte[]
        {
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,1
        };
        
        internal ICodec<T> InnerCodec;

        public ArrayCodec(ICodec<T> innerCodec)
        {
            InnerCodec = innerCodec;
        }

        public override T?[] Deserialize(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // skip reserved
            reader.Skip(8);

            if (dimensions is 0)
            {
                return Array.Empty<T>();
            }

            if(dimensions is not 1)
            {
                throw new NotSupportedException("Only dimensions of 1 are supported for arrays");
            }

            var upper = reader.ReadInt32();
            var lower = reader.ReadInt32();

            var numElements = upper - lower + 1;

            T?[] array = new T[numElements];

            for(int i = 0; i != numElements; i++)
            {
                var elementLength = reader.ReadInt32();
                reader.ReadBytes(elementLength, out var innerData);
                array[i] = InnerCodec.Deserialize(innerData);
            }

            return array;
        }

        public override void Serialize(ref PacketWriter writer, T?[]? value)
        {
            if(value is null)
            {
                writer.Write(EMPTY_ARRAY);
                return;
            }

            writer.Write(1); // num dimensions
            writer.Write(0); // reserved
            writer.Write(0); // reserved

            // dimension (our length for upper and 1 for lower)
            writer.Write(value.Length); 
            writer.Write(1);
            
            for(int i = 0; i != value.Length; i++)
            {
                var element = value[i];

                if(element is null)
                {
                    writer.Write(-1);
                }
                else
                {
                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => InnerCodec.Serialize(ref innerWriter, element));
                }
            }
        }

        ICodec IWrappingCodec.InnerCodec
        {
            get => InnerCodec;
            set
            {
                if (value is null)
                    throw new NullReferenceException("Attempted to supply a 'null' instance codec to a wrapping codec");

                if (value is not ICodec<T> correctedValue)
                    throw new NotSupportedException($"Cannot set {value} as a Codec<T>");

                InnerCodec = correctedValue;
            }
        }
    }
}
