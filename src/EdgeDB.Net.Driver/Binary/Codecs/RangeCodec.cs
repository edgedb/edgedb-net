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

        public void Serialize(PacketWriter writer, Range<T> value)
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
                var lowerWriter = new PacketWriter();
                _innerCodec.Serialize(value.Lower.Value);
                writer.Write((int)lowerWriter.Length);
                writer.Write(lowerWriter);
            }

            if (value.Upper.HasValue)
            {
                var upperWriter = new PacketWriter();
                _innerCodec.Serialize(value.Upper.Value);
                writer.Write((int)upperWriter.Length);
                writer.Write(upperWriter);
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
