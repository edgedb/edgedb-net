namespace EdgeDB.Codecs
{
    internal struct Element
    {
        public int Reserved { get; set; }

        public int Length { get; set; }

        public byte[] Data { get; set; }
    }
}
