namespace EdgeDB.Binary.Codecs;

internal sealed class ArgumentCodecContext : CodecContext
{
    public readonly ICodec[] Codecs;

    public ArgumentCodecContext(ICodec[] codecs, GelBinaryClient client)
        : base(client)
    {
        Codecs = codecs;
    }
}
