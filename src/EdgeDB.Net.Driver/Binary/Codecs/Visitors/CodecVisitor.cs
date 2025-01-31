using Gel.Utils;

namespace Gel.Binary.Codecs;

internal abstract class CodecVisitor
{
    public Task VisitAsync(Ref<ICodec> codec, CancellationToken token) => VisitCodecAsync(codec, token);

    protected abstract Task VisitCodecAsync(Ref<ICodec> codec, CancellationToken token);
}
