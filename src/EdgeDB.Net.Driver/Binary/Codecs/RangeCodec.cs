using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal class RangeCodec<T> : ICodec<Range<T>>
        where T : struct
    {
        private ICodec<T> _innerCodec;
        public RangeCodec(ICodec<T> innerCodec)
        {
            _innerCodec = innerCodec;
        }

        public Range<T> Deserialize(ref PacketReader reader)
        {
            var flags = (RangeFlags)reader.ReadByte();

            if ((flags & RangeFlags.Empty) != 0)
                return Range<T>.Empty();

            T? lowerBound = null, upperBound = null;


            if((flags & RangeFlags.InfiniteLowerBound) == 0)
            {
                reader.Skip(4);
                lowerBound = _innerCodec.Deserialize(ref reader);
            }

            if ((flags & RangeFlags.IncludeUpperBound) == 0)
            {
                reader.Skip(4);
                upperBound = _innerCodec.Deserialize(ref reader);
            }

            return new Range<T>(lowerBound, upperBound, (flags & RangeFlags.IncudeLowerBound) != 0, (flags & RangeFlags.IncludeUpperBound) != 0);
        }

        public void Serialize(ref PacketWriter writer, Range<T> value)
        {
            var flags = value.IsEmpty
                ? RangeFlags.Empty
                : (value.IncludeLower ? RangeFlags.IncudeLowerBound : 0) |
                  (value.IncludeUpper ? RangeFlags.IncludeUpperBound : 0) |
                  (!value.Lower.HasValue ? RangeFlags.InfiniteLowerBound : 0) |
                  (!value.Upper.HasValue ? RangeFlags.InfiniteUpperBound : 0);

            writer.Write((byte)flags);
            
            if(value.Lower.HasValue)
            {
                writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => _innerCodec.Serialize(ref innerWriter, value.Lower.Value));
            }

            if (value.Upper.HasValue)
            {
                writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => _innerCodec.Serialize(ref innerWriter, value.Upper.Value));
            }
        }

        [Flags]
        public enum RangeFlags : byte
        {
            Empty = 1 << 0,
            IncudeLowerBound = 1 << 1,
            IncludeUpperBound = 1 << 2,
            InfiniteLowerBound = 1 << 3,
            InfiniteUpperBound = 1 << 4,
        }
    }
}
