namespace EdgeDB.Codecs
{
    internal class Array<TInner> : ICodec<IEnumerable<TInner?>>
    {
        private readonly ICodec<TInner> _innerCodec;

        public Array(ICodec<TInner> innerCodec)
        {
            _innerCodec = innerCodec;
        }

        public IEnumerable<TInner?> Deserialize(PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // skip reserved
            reader.ReadBytes(8);

            if (dimensions == 0)
            {
                return Array.Empty<TInner>();
            }

            if(dimensions != 1)
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
                var innerData = reader.ReadBytes(elementLength);

                // TODO: optimize this stream creation?
                using(var innerReader = new PacketReader(innerData))
                {
                    array[i] = _innerCodec.Deserialize(innerReader);
                }
            }

            return array;
        }

        public void Serialize(PacketWriter writer, IEnumerable<TInner?>? value)
        {
            if(value == null)
            {
                writer.Write(0);
                writer.Write(0); // flags?
                writer.Write(0); // reserved
                writer.Write(0); // zero upper
                writer.Write(1); // one lower
                return;
            }

            var elements = value.ToArray(); // convert to array to prevent the reference from changing while we serialize

            var elementWriter = new PacketWriter();

            for(int i = 0; i != elements.Length; i++)
            {
                var element = elements[i];

                if(element == null)
                {
                    elementWriter.Write(-1);
                }
                else
                {
                    _innerCodec.Serialize(elementWriter, element);
                }
            }

            writer.Write(1); // dimensions
            writer.Write(0); // flags?
            writer.Write(0); // reserved

            // dimension (our length for upper and 1 for lower)
            writer.Write(elements.Length); 
            writer.Write(1);

            // write our serialized elements
            writer.Write(elementWriter);
        }
    }
}
