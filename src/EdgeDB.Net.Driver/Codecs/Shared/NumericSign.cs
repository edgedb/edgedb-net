namespace EdgeDB.Codecs
{
    internal enum NumericSign : ushort
    {
        // Positive value.
        POS = 0x0000,

        // Negative value.
        NEG = 0x4000
    };
}
