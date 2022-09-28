namespace EdgeDB.Binary.Codecs
{
    internal sealed class Array<TInner> : ICodec<IEnumerable<TInner?>>
    {
        public static readonly byte[] EMPTY_ARRAY = new byte[]
        {
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,0,
            0,0,0,1
        };
        
        private readonly ICodec<TInner> _innerCodec;

        public Array(ICodec<TInner> innerCodec)
        {
            _innerCodec = innerCodec;
        }

        public IEnumerable<TInner?> Deserialize(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // skip reserved
            reader.Skip(8);

            if (dimensions is 0)
            {
                return Array.Empty<TInner>();
            }

            if(dimensions is not 1)
            {
                throw new NotSupportedException("Only dimensions of 1 are supported for arrays");
            }

            var upper = reader.ReadInt32();
            var lower = reader.ReadInt32();

            var numElements = upper - lower + 1;

            TInner?[] array = new TInner[numElements];

            for(int i = 0; i != numElements; i++)
            {
                var elementLength = reader.ReadInt32();
                reader.ReadBytes(elementLength, out var innerData);
                array[i] = _innerCodec.Deserialize(innerData);
            }

            return array;
        }

        public void Serialize(ref PacketWriter writer, IEnumerable<TInner?>? value)
        {
            if(value is null)
            {
                writer.Write(EMPTY_ARRAY);
                return;
            }

            var elements = value.ToArray(); // convert to array to prevent the reference from changing while we serialize

            writer.Write(1); // num dimensions
            writer.Write(0); // reserved
            writer.Write(0); // reserved

            // dimension (our length for upper and 1 for lower)
            writer.Write(elements.Length); 
            writer.Write(1);
            
            for(int i = 0; i != elements.Length; i++)
            {
                var element = elements[i];

                if(element is null)
                {
                    writer.Write(-1);
                }
                else
                {
                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => _innerCodec.Serialize(ref innerWriter, element));
                }
            }
        }
    }
}
