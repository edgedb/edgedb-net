using EdgeDB.Utils;

namespace EdgeDB.Binary.Codecs;

internal abstract class CodecVisitor
{
    public Task VisitAsync(Ref<ICodec> codec) => VisitCodecAsync(codec);

    protected abstract Task VisitCodecAsync(Ref<ICodec> codec);
}
