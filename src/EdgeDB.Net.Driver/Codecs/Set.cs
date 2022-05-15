namespace EdgeDB.Codecs
{
    internal class Set<TInner> : ICodec<IEnumerable<TInner?>>
    {
        private readonly ICodec<TInner> _innerCodec;

        public Set(ICodec<TInner> innerCodec)
        {
            _innerCodec = innerCodec;
        }

        public IEnumerable<TInner?>? Deserialize(PacketReader reader)
        {
            if (_innerCodec is Array<TInner>)
                return DecodeSetOfArrays(reader);
            else return DecodeSet(reader);
        }

        public void Serialize(PacketWriter writer, IEnumerable<TInner?>? value)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<TInner?>? DecodeSetOfArrays(PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.ReadBytes(8);

            if(dimensions == 0)
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

            var result = new TInner?[numElements];

            for(int i = 0; i != numElements; i++)
            {
                reader.ReadBytes(4); // skip array element size

                var envelopeElements = reader.ReadInt32();

                if(envelopeElements != 1)
                {
                    throw new InvalidDataException($"Envelope should contain 1 element, got {envelopeElements} elements");
                }

                reader.ReadBytes(4); // skip reserved

                result[i] = _innerCodec.Deserialize(reader);
            }

            return result;
        }

        private IEnumerable<TInner?>? DecodeSet(PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.ReadBytes(8);

            if (dimensions == 0)
            {
                return Array.Empty<TInner>();
            }

            if (dimensions != 1)
            {
                throw new NotSupportedException("Only dimensions of 1 are supported for sets");
            }

            var upper = reader.ReadInt32();
            var lower = reader.ReadInt32();

            var numElements = upper - lower + 1;

            var result = new TInner?[numElements];

            for(int i = 0; i != numElements; i++)
            {
                var elementLength = reader.ReadInt32();

                if (elementLength == -1)
                    result[i] = default; // TODO: better 'null' value handling?
                else
                    result[i] = _innerCodec.Deserialize(reader);
            }

            return result;
        }
    }
}
