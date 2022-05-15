namespace EdgeDB.Codecs
{
    internal class Decimal : IScalarCodec<decimal>
    {
        public const decimal NBASE = 10000;

        public decimal Deserialize(PacketReader reader)
        {
            var numDigits = reader.ReadUInt16();
            var weight = reader.ReadInt16();
            var sign = (NumericSign)reader.ReadUInt16();
            var displayScale = reader.ReadUInt16();

            string value = "";

            if (sign == NumericSign.NEG)
                value += "-";

            int d;

            if (weight < 0)
            {
                d = weight + 1;
                value += "0";
            }
            else
            {
                for (d = 0; d <= weight; d++)
                {
                    var digit = d < numDigits ? reader.ReadUInt16() : 0;
                    var sdigit = $"{digit}";
                    if (d > 0)
                        sdigit = sdigit.PadLeft(4, '0');
                    value += sdigit;
                }
            }

            if (displayScale > 0)
            {
                value += ".";
                var end = value.Length + displayScale;
                for (int i = 0; i < displayScale; d++, i += 4)
                {
                    var digit = d >= 0 && d < numDigits ? reader.ReadUInt16() : 0;
                    value += digit.ToString().PadLeft(4, '0');
                }

                value = value[..end];
            }

            return decimal.Parse(value);
        }

#pragma warning disable IDE0022 // Use expression body for methods
        public void Serialize(PacketWriter writer, decimal value)
        {
            // TODO https://www.edgedb.com/docs/reference/protocol/dataformats#std-decimal
            throw new NotSupportedException();

            //var sign = Value > 0 ? DecimalSign.POS : DecimalSign.NEG;
            //var uval = Value;

            //if (Value == 0)
            //{
            //    writer.Write(0u);
            //    writer.Write((ushort)sign);
            //    writer.Write((ushort)0);
            //    return;
            //}

            //if (sign == DecimalSign.NEG)
            //    uval = -uval;

            //List<ushort> digits = new();

            //while(Math.Abs(uval) != 0)
            //{
            //    var mod = uval % NBASE;
            //    uval /= NBASE;

            //    digits.Add((ushort)mod);
            //}

            //writer.Write((ushort)digits.Count);
            //writer.Write((ushort)digits.Count - 1);
            //writer.Write((ushort)sign);
            //writer.Write((ushort)0);
            //for(int i = digits.Count - 1; i >= 0; i--)
            //{
            //    writer.Write(digits[i]);
            //}
        }
#pragma warning restore IDE0022 // Use expression body for methods
    }
}
