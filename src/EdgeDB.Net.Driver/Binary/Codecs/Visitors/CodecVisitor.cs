namespace EdgeDB.Binary.Codecs;

internal abstract class CodecVisitor
{
    public void Visit(ref ICodec codec) => VisitCodec(ref codec);

    protected abstract void VisitCodec(ref ICodec codec);
}
