namespace EdgeDB.Binary.Codecs
{
    internal sealed class SetCodec<T>
        : BaseCodec<IEnumerable<T?>>, IWrappingCodec, ICacheableCodec
    {
        internal ICodec<T> InnerCodec;

        public SetCodec(ICodec<T> innerCodec)
        {
            InnerCodec = innerCodec;
        }

        public override IEnumerable<T?>? Deserialize(ref PacketReader reader)
        {
            if (InnerCodec is ArrayCodec<T>)
                return DecodeSetOfArrays(ref reader);
            else return DecodeSet(ref reader);
        }

        public override void Serialize(ref PacketWriter writer, IEnumerable<T?>? value)
        {
            throw new NotSupportedException();
        }

        private IEnumerable<T?>? DecodeSetOfArrays(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.Skip(8);

            if(dimensions is 0)
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

            var result = new T?[numElements];

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

        private IEnumerable<T?>? DecodeSet(ref PacketReader reader)
        {
            var dimensions = reader.ReadInt32();

            // discard flags and reserved
            reader.Skip(8);

            if (dimensions is 0)
            {
                return Array.Empty<T>();
            }

            if (dimensions is not 1)
            {
                throw new NotSupportedException("Only dimensions of 1 are supported for sets");
            }

            var upper = reader.ReadInt32();
            var lower = reader.ReadInt32();

            var numElements = upper - lower + 1;

            var result = new T?[numElements];

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
