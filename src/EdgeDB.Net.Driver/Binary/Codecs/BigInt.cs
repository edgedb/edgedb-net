using System.Numerics;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class BigInt : BaseScalarCodec<BigInteger>
    {
        public static readonly BigInteger Base = 10000;

        public override BigInteger Deserialize(ref PacketReader reader)
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

        public override void Serialize(ref PacketWriter writer, BigInteger value)
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
            var mutableValue = value;
            List<ushort> digits = new();
            
            while(mutableValue != 0)
            {
                var mod = absValue % Base;
                mutableValue /= Base;
                digits.Add((ushort)mod);
            }

            writer.Write((ushort)digits.Count); // ndigits
            writer.Write((ushort)digits.Count - 1); // weight
            writer.Write((ushort)sign); // sign
            writer.Write((ushort)0); // reserved
            for (int i = 0; i != digits.Count; i++)
                writer.Write(digits[i]);
        }
    }
}
