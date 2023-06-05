using EdgeDB.Utils;
using System.Globalization;

namespace EdgeDB.Binary.Codecs
{
    // TODO: get rid of ugly strings and convert the Win32 DECIMAL to correct format
    internal sealed class DecimalCodec
        : BaseScalarCodec<decimal>
    {
        public const int NBASE = 10000;

        public override decimal Deserialize(ref PacketReader reader, CodecContext context)
        {
            var numDigits = reader.ReadUInt16();
            var weight = reader.ReadInt16();
            var sign = (NumericSign)reader.ReadUInt16();
            var displayScale = reader.ReadUInt16();

            string value = "";

            if (sign is NumericSign.NEG)
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
                value += CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
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

        public override void Serialize(ref PacketWriter writer, decimal value, CodecContext context)
        {
#if LEGACY
            var bits = decimal.GetBits(value);
#else
            Span<int> bits = stackalloc int[4];
            decimal.GetBits(value, bits);
#endif

            var rawDscale = (bits[3] >> 16) & 0x7F;
            var rawSign = (bits[3] >> 31) & 0x01;

            var str = value.ToString();
            var spl = str.Split(CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);


            var integral = spl[0][0] == '-' ? spl[0][1..] : spl[0];
            var frac = spl.Length > 1 ? spl[1] : string.Empty;

            var sdigits =
                integral.PadLeft((int)(Math.Ceiling(integral.Length / 4d) * 4), '0') +
                frac.PadRight((int)(Math.Ceiling(frac.Length / 4d) * 4), '0');

            List<ushort> digits = new();

            for(int i = 0, len = sdigits.Length; i < len; i+=4)
            {
                digits.Add(ushort.Parse(sdigits[i..(i + 4)]));
            }

            var ndigits = (ushort)digits.Count;
            var weight = (short)(Math.Ceiling(integral.Length / 4d) - 1);
            var sign = (ushort)(rawSign == 1 ? NumericSign.NEG : NumericSign.POS);
            var dscale = (short)frac.Length;

            writer.Write(ndigits);
            writer.Write(weight);
            writer.Write(sign);
            writer.Write(dscale);

            foreach (var digit in digits)
                writer.Write(digit);
        }
    }
}
