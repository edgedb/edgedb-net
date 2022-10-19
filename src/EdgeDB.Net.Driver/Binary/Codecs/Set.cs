namespace EdgeDB.Binary.Codecs
{
    internal sealed class Set<TInner> : ICodec<IEnumerable<TInner?>>, IWrappingCodec
    {
        internal readonly ICodec<TInner> InnerCodec;

        public Set(ICodec<TInner> innerCodec)
        {
            InnerCodec = innerCodec;
        }

        public IEnumerable<TInner?>? Deserialize(ref PacketReader reader)
        {
            if (InnerCodec is Array<TInner>)
                return DecodeSetOfArrays(ref reader);
            else return DecodeSet(ref reader);
        }

        public void Serialize(ref PacketWriter writer, IEnumerable<TInner?>? value)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<TInner?>? DecodeSetOfArrays(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.Skip(8);

            if(dimensions is 0)
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

            var result = new TInner?[numElements];

            for(int i = 0; i != numElements; i++)
            {
                reader.Skip(4); // skip array element size

                var envelopeElements = reader.ReadInt32();

                if(envelopeElements != 1)
                {
                    throw new InvalidDataException($"Envelope should contain 1 element, got {envelopeElements} elements");
                }

                reader.Skip(4); // skip reserved

                result[i] = InnerCodec.Deserialize(ref reader);
            }

            return result;
        }

        private IEnumerable<TInner?>? DecodeSet(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.Skip(8);

            if (dimensions is 0)
            {
                return Array.Empty<TInner>();
            }

            if (dimensions is not 1)
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

                if (elementLength is -1)
                    result[i] = default;
                else
                    result[i] = InnerCodec.Deserialize(ref reader);
            }

            return result;
        }

        ICodec IWrappingCodec.InnerCodec => InnerCodec;
    }
}
