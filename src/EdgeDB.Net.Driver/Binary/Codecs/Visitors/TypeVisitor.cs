using EdgeDB.DataTypes;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using DateTime = System.DateTime;

namespace EdgeDB.Binary.Codecs;

internal sealed class TypeVisitor : CodecVisitor
{
    private readonly EdgeDBBinaryClient _client;

    private readonly Stack<TypeResultFrame> _frames;
    private readonly ILogger _logger;

    public TypeVisitor(EdgeDBBinaryClient client)
    {
        _frames = new Stack<TypeResultFrame>();
        _logger = client.Logger;
        _client = client;
    }

    private TypeResultFrame Context
        => _frames.Peek();

    private string Depth
        => "".PadRight(_frames.Count, '>') + (_frames.Count > 0 ? " " : "");

    public void SetTargetType(Type type)
    {
        EdgeDBTypeDeserializeInfo? deserializer = null;

        if (type != typeof(void))
        {
            _ = TypeBuilder.IsValidObjectType(type) && TypeBuilder.TryGetTypeDeserializerInfo(type, out deserializer);
        }

        var old = _frames.Count == 0 ? typeof(void) : Context.Type;

        _logger.CodecVisitorStackTransition(Depth, old, type);

        _frames.Push(new TypeResultFrame {Type = type, Deserializer = deserializer});
    }

    public void Reset()
        => _frames.Clear();

    protected override void VisitCodec(ref ICodec codec)
    {
        // TODO: if dynamic or object was passed in, return the default type
        // from the complex OR based on config.

        if (Context.Type == typeof(void))
            return;

        _logger.CodecVisitorNewCodec(Depth, codec);

        switch (codec)
        {
            case ObjectCodec obj:
            {
                if (obj is TypeInitializedObjectCodec typeCodec)
                {
                    if (typeCodec.TargetType != Context.Type)
                        typeCodec = typeCodec.Parent.GetOrCreateTypeCodec(Context.Type);
                }
                else
                {
                    typeCodec = obj.GetOrCreateTypeCodec(Context.Type);
                }

                if (typeCodec.TargetType is null || typeCodec.Deserializer is null)
                    throw new NullReferenceException("Could not find deserializer info for object codec.");

                using var objHandle = EnterNewContext(typeCodec.TargetType, typeCodec.TargetType.Name,
                    typeCodec.Deserializer);
                for (var i = 0; i != obj.InnerCodecs.Length; i++)
                {
                    ref var innerCodec = ref obj.InnerCodecs[i];
                    var name = obj.PropertyNames[i];

                    // use the defined type, if not found, use the codecs type
                    // if the inner is compilable, use its inner type and set the real
                    // flag, since compileable visits only care about the inner type rather
                    // than a concrete root.
                    var hasPropInfo = Context.Deserializer!.PropertyMapInfo.Map.TryGetValue(name, out var propInfo);

                    var propType = hasPropInfo
                        ? propInfo!.Type
                        : innerCodec is CompilableWrappingCodec compilable
                            ? compilable.GetInnerType()
                            : innerCodec.ConverterType;

                    using var propHandle = EnterNewContext(propType, name,
                        innerRealType: !hasPropInfo && innerCodec is CompilableWrappingCodec);

                    Visit(ref innerCodec);

                    _logger.CodecVisitorMutatedCodec(Depth, obj.InnerCodecs[i], innerCodec);
                }

                codec = typeCodec;
            }
                break;
            case TupleCodec tuple:
            {
                var tupleTypes = Context.Type.IsAssignableTo(typeof(ITuple)) && Context.Type != typeof(TransientTuple)
                    ? TransientTuple.FlattenTypes(Context.Type)
                    : null;

                if (tupleTypes is not null && tupleTypes.Length != tuple.InnerCodecs.Length)
                    throw new NoTypeConverterException($"Cannot determine inner types of the tuple {Context.Type}");

                var type = typeof(object);

                for (var i = 0; i != tuple.InnerCodecs.Length; i++)
                {
                    if (tupleTypes != null)
                        type = tupleTypes[i];

                    var innerCodec = tuple.InnerCodecs[i];
                    using var elementHandle = EnterNewContext(type);

                    Visit(ref innerCodec);

                    _logger.CodecVisitorMutatedCodec(Depth, tuple.InnerCodecs[i], innerCodec);

                    tuple.InnerCodecs[i] = innerCodec;
                }

                codec = tuple.GetCodecFor(_client.ProtocolProvider, GetContextualTypeForComplexCodec(tuple));
                _logger.CodecVisitorComplexCodecFlattened(Depth, tuple, codec, Context.Type);
            }
                break;
            case CompilableWrappingCodec compilable:
            {
                // visit the inner codec
                var tmp = compilable.InnerCodec;

                var innerType = Context.InnerRealType || Context.IsDynamicResult
                    ? Context.Type
                    : Context.Type.GetWrappingType();

                using (var innerHandle = EnterNewContext(innerType))
                {
                    VisitCodec(ref tmp);

                    codec = compilable.Compile(_client.ProtocolProvider, Context.Type, tmp);

                    _logger.CodecVisitorCompiledCodec(Depth, compilable, codec, Context.Type);
                }

                _logger.CodecVisitorMutatedCodec(Depth, tmp, codec);

                // visit the compiled codec
                Visit(ref codec);
            }
                break;
            case IComplexCodec complex:
            {
                codec = complex.GetCodecFor(_client.ProtocolProvider, GetContextualTypeForComplexCodec(complex));
                _logger.CodecVisitorComplexCodecFlattened(Depth, complex, codec, Context.Type);
            }
                break;
            case IRuntimeCodec runtime:
            {
                // check if the converter type is the same.
                // exit if they are: its the correct codec.
                if (runtime.ConverterType == Context.Type)
                    break;

                // ask the broker of the runtime codec for
                // the correct one.
                codec = runtime.Broker.GetCodecFor(_client.ProtocolProvider,
                    GetContextualTypeForComplexCodec(runtime.Broker));

                _logger.CodecVisitorRuntimeCodecBroker(Depth, runtime, runtime.Broker, codec, Context.Type);
            }
                break;
        }
    }

    private Type GetContextualTypeForComplexCodec(IComplexCodec codec)
    {
        // if theres a concrete type def supplied by the
        // user, return their requested type.
        if (!Context.IsDynamicResult && !Context.InnerRealType)
            return Context.Type;

        if (codec is ITemporalCodec temporal)
        {
            // check if we should default to the model type or a system type
            if (!_client.ClientConfig.PreferSystemTemporalTypes)
            {
                // use model type
                return temporal.ModelType;
            }

            // use a supported system type
            return temporal switch
            {
                DateDurationCodec => typeof(TimeSpan),
                DateTimeCodec => typeof(DateTimeOffset),
                DurationCodec => typeof(TimeSpan),
                LocalDateCodec => typeof(DateOnly),
                LocalDateTimeCodec => typeof(DateTime),
                LocalTimeCodec => typeof(TimeOnly),
                RelativeDurationCodec => typeof(TimeSpan),
                _ => throw new NotSupportedException(
                    $"Cannot find a valid .NET system temporal type for the codec {temporal}")
            };
        }

        if (codec.GetType().IsGenericType && codec.GetType().GetGenericTypeDefinition() == typeof(RangeCodec<>))
        {
            // always prefer the default converter for range
            return codec.ConverterType;
        }

        if (codec is TupleCodec tpl)
        {
            if (_client.ClientConfig.PreferValueTupleType)
                return tpl.CreateValueTupleType();

            return typeof(TransientTuple);
        }

        // return out the current context type if we haven't
        // defined a way to change it.
        return Context.Type;
    }

    private IDisposable EnterNewContext(
        Type type,
        string? name = null,
        EdgeDBTypeDeserializeInfo? deserializer = null,
        bool innerRealType = false)
    {
        var old = _frames.Count == 0 ? typeof(void) : Context.Type;

        _logger.CodecVisitorStackTransition(Depth, old, type);

        _frames.Push(new TypeResultFrame
        {
            Type = type, Name = name, Deserializer = deserializer, InnerRealType = innerRealType
        });


        return new FrameHandle(_logger, Depth, _frames);
    }

    private struct TypeResultFrame
    {
        public bool IsDynamicResult
            => Type == typeof(object);

        public readonly Type Type { get; init; }
        public string? Name { get; set; }
        public EdgeDBTypeDeserializeInfo? Deserializer { get; set; }
        public bool InnerRealType { get; set; }
    }

    private readonly struct FrameHandle : IDisposable
    {
        private readonly Stack<TypeResultFrame> _stack;
        private readonly ILogger _logger;
        private readonly string _depth;


        public FrameHandle(ILogger logger, string depth, Stack<TypeResultFrame> stack)
        {
            _depth = depth;
            _logger = logger;
            _stack = stack;
        }

        public void Dispose() =>
            _logger.CodecVisitorFramePopped(_depth == string.Empty ? string.Empty : $"{_depth[..^2]} ",
                _stack.Pop().Type);
    }
}
