using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.Utils;
using System.Numerics;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class BigIntCodec
        : BaseScalarCodec<BigInteger>
    {
        public new static readonly Guid Id = Guid.Parse("00000000-0000-0000-0000-000000000108");

        public static readonly BigInteger Base = 10000;

        public BigIntCodec(CodecMetadata? metadata = null)
            : base(in Id, metadata)
        { }

        public override BigInteger Deserialize(ref PacketReader reader, CodecContext context)
        {
            var numDigits = reader.ReadUInt16();

            var weight = reader.ReadInt16();

            var sign = (NumericSign)reader.ReadUInt16();

            // reserved
            reader.Skip(2);

            string result = sign == NumericSign.NEG ? "-" : "";

            int i = weight, d = 0;

            while (i >= 0)
            {
                if (i <= weight && d < numDigits)
                {
                    var digit = reader.ReadUInt16().ToString();
                    result += d > 0 ? digit.PadLeft(4, '0') : digit;
                    d++;
                }
                else
                {
                    result += "0000";
                }
                i--;
            }

            return BigInteger.Parse(result);
        }

        public override void Serialize(ref PacketWriter writer, BigInteger value, CodecContext context)
        {
            if(value == 0)
            {
                writer.Write(0u); // ndigits & weight
                writer.Write((ushort)NumericSign.POS);
                writer.Write((ushort)0); // reserved
                return;
            }

            var sign = value > 0 ? NumericSign.POS : NumericSign.NEG;
            var absValue = value < 0 ? -value : value;
            List<ushort> digits = new();
            
            while(absValue != 0)
            {
                var mod = absValue % Base;
                absValue /= Base;
                digits.Add((ushort)mod);
            }

            writer.Write((ushort)digits.Count); // ndigits
            writer.Write((short)(digits.Count - 1)); // weight
            writer.Write((ushort)sign); // sign
            writer.Write((ushort)0); // reserved
            for (int i = digits.Count - 1; i >= 0; i--)
                writer.Write(digits[i]);
        }

        public override string ToString()
            => "std::bigint";
    }
}
