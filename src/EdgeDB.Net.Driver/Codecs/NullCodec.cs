namespace EdgeDB.Codecs
{
    internal class NullCodec : ICodec, IArgumentCodec
    {
        public Type ConverterType => typeof(object);

        public bool CanConvert(Type t)
        {
            return true;
        }

        public object? Deserialize(PacketReader reader) { return null; }

        public void Serialize(PacketWriter writer, object? value)
        {
            writer.Write(0);
        }

        public void SerializeArguments(PacketWriter writer, object? value)
        {
            writer.Write(0);
        }
    }
}
