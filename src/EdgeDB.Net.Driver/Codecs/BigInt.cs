using System.Numerics;

namespace EdgeDB.Codecs
{
    internal class BigInt : IScalarCodec<BigInteger>
    {
        public BigInteger Deserialize(PacketReader reader)
        {
            var numDigits = reader.ReadUInt16();

            var weight = reader.ReadInt16();

            var sign = (NumericSign)reader.ReadUInt16();

            // reserved
            reader.ReadUInt16();

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

        public void Serialize(PacketWriter writer, BigInteger value) => throw new NotImplementedException();
    }
}
