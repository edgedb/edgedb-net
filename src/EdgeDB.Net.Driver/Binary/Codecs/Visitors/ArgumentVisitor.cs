using EdgeDB.Utils;

namespace EdgeDB.Binary.Codecs;

internal sealed class ArgumentVisitor : CodecVisitor
{
    public ICodec[] VisitedChildCodecs { get; private set; } = Array.Empty<ICodec>();
    private readonly IDictionary<string, object?> _arguments;
    private readonly EdgeDBBinaryClient _client;

    public ArgumentVisitor(EdgeDBBinaryClient client, IDictionary<string, object?> args)
    {
        _client = client;
        _arguments = args;
    }

    protected override async Task VisitCodecAsync(Ref<ICodec> codec, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        switch (codec.Value)
        {
            case ObjectCodec objectCodec:
                VisitedChildCodecs = await Task.WhenAll(
                    objectCodec.InnerCodecs.Select(async (x, i) =>
                    {
                        var type = typeof(object);

                        if (_arguments.TryGetValue(objectCodec.PropertyNames[i], out var v))
                            type = v?.GetType() ?? typeof(object);

                        var reference = new Ref<ICodec>(x);
                        await VisitAsync(reference, type, token);

                        return reference.Value;
                    })
                );
                break;
            case NullCodec:
                break;
            default:
                throw new InvalidOperationException($"Got unexpected argument codec \"{codec.Value}\"");
        }
    }

    private async ValueTask VisitAsync(Ref<ICodec> codec, Type type, CancellationToken token)
    {
        var typeVisitor = new TypeVisitor(_client);
        typeVisitor.SetTargetType(type);
        await typeVisitor.VisitAsync(codec, token);
    }
}
