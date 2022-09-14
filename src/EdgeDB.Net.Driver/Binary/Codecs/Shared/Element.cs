namespace EdgeDB.Binary.Codecs
{
    internal readonly struct Element
    {
        public int Reserved { get; init; }

        public int Length { get; init; }

        public byte[] Data { get; init; }
    }
}
