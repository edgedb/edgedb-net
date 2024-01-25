using EdgeDB.Binary.Builders.Wrappers;
using EdgeDB.DataTypes;
using EdgeDB.Utils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DateTime = System.DateTime;

namespace EdgeDB.Binary.Codecs;

internal sealed class TypeVisitor : CodecVisitor
{
    private readonly EdgeDBBinaryClient _client;

    private readonly ILogger _logger;

    private Type? _targetType;
    private TypeVisitorContext? _context;

    public TypeVisitor(EdgeDBBinaryClient client)
    {
        _logger = client.Logger;
        _client = client;
    }

    public void SetTargetType(Type type)
    {
        _targetType = type;
        _context = new TypeVisitorContext(type);
    }

    protected override
#if DEBUG
        async
#endif
        Task VisitCodecAsync(Ref<ICodec> codec, CancellationToken token)
    {
        if (_context is null)
            throw new EdgeDBException("Context was not initialized for type walking");

#if DEBUG
        var sw = Stopwatch.StartNew();
        await VisitCodecAsync(codec, _context, token);
        sw.Stop();
        _logger.CodecVisitorTimingTrace(
            this,
            Math.Round(sw.Elapsed.TotalMilliseconds, 4),
            CodecFormatter.FormatCodecAsTree(codec.Value).ToString()
        );
#else
        if(_logger.IsEnabled(LogLevel.Trace))
            _logger.CodecTree(CodecFormatter.FormatCodecAsTree(codec.Value).ToString());
        return VisitCodecAsync(codec, _context, token);
#endif
    }

    private async Task VisitCodecAsync(Ref<ICodec> codec, TypeVisitorContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (context.Type == typeof(void))
            return;

        _logger.CodecVisitorNewCodec(context.FormattedDepth, codec.Value);

        switch (codec.Value)
        {
            case ICompiledCodec compiled when (context.Type != compiled.ConverterType):
            {
                var reference = new Ref<ICodec>(compiled.InnerCodec);

                var type = context.IsDynamicResult
                    ? context.Type
                    : context.Type.GetWrappingType();

                await VisitCodecAsync(reference, context with { Type = type }, token);
                compiled.InnerCodec = reference.Value;
            }
                break;
            case ObjectCodec obj:
            {
                if (obj is TypeInitializedObjectCodec typeCodec)
                {
                    if (typeCodec.TargetType != context.Type)
                        typeCodec = typeCodec.Parent.GetOrCreateTypeCodec(context.Type);
                }
                else
                {
                    typeCodec = obj.GetOrCreateTypeCodec(context.Type);
                }

                if (typeCodec.TargetType is null || typeCodec.Deserializer is null)
                    throw new NullReferenceException("Could not find deserializer info for object codec.");

                var subContext = context with
                {
                    Type = typeCodec.TargetType,
                    Deserializer = typeCodec.Deserializer,
                    Name = typeCodec.TargetType.Name
                };

                await Task.WhenAll(obj.InnerCodecs.Select(async (x, i) =>
                {
                    // use the defined type, if not found, use the codecs type
                    // if the inner is compilable, use its inner type and set the real
                    // flag, since compilable visits only care about the inner type rather
                    // than a concrete root.
                    var hasPropInfo =
                        typeCodec.Deserializer.PropertyMapInfo.Map.TryGetValue(obj.PropertyNames[i], out var propInfo);

                    var reference = new Ref<ICodec>(x);

                    await VisitCodecAsync(reference, subContext with
                    {
                        Depth = subContext.Depth + 1,
                        Type = hasPropInfo
                            ? propInfo!.Type
                            : x is CompilableWrappingCodec compilable
                                ? compilable.GetInnerType()
                                : x.ConverterType,
                        InnerRealType = !hasPropInfo && x is CompilableWrappingCodec,
                        Name = obj.PropertyNames[i]
                    }, token);

                    obj.InnerCodecs[i] = reference.Value;
                }));

                codec.Value = typeCodec;
            }
            break;
            case TupleCodec tuple:
            {
                var tupleTypes = context.Type.IsAssignableTo(typeof(ITuple)) && context.Type != typeof(TransientTuple)
                    ? TransientTuple.FlattenTypes(context.Type)
                    : null;

                if (tupleTypes is not null && tupleTypes.Length != tuple.InnerCodecs.Length)
                    throw new NoTypeConverterException($"Cannot determine inner types of the tuple {context.Type}");


                await Task.WhenAll(tuple.InnerCodecs.Select(async (x, i) =>
                {
                    var reference = new Ref<ICodec>(x);
                    await VisitCodecAsync(reference, context with
                    {
                        Type = tupleTypes is not null
                            ? tupleTypes[i]
                            : typeof(object)
                    }, token);
                    tuple.InnerCodecs[i] = reference.Value;
                }));

                codec.Value = tuple.GetCodecFor(_client.ProtocolProvider, GetContextualTypeForComplexCodec(tuple, context));
            }
                break;
            case CompilableWrappingCodec compilable:
            {
                // visit the inner codec
                var tmp = compilable.InnerCodec;

                var innerType = context.InnerRealType || context.IsDynamicResult
                    ? context.Type
                    : context.Type.GetWrappingType();

                var reference = new Ref<ICodec>(compilable.InnerCodec);
                await VisitCodecAsync(reference, context with { Type = innerType }, token);
                codec.Value = compilable.Compile(_client.ProtocolProvider, innerType, reference.Value);
            }
                break;
            case IComplexCodec complex:
            {
                codec.Value = complex.GetCodecFor(_client.ProtocolProvider, GetContextualTypeForComplexCodec(complex, context));
            }
                break;
            case IRuntimeCodec runtime:
            {
                // check if the converter type is the same.
                // exit if they are: its the correct codec.
                if (runtime.ConverterType == context.Type)
                    break;

                // ask the broker of the runtime codec for
                // the correct one.
                codec.Value = runtime.Broker.GetCodecFor(_client.ProtocolProvider,
                    GetContextualTypeForComplexCodec(runtime.Broker, context));

            }
                break;
        }
    }

    private Type GetContextualTypeForComplexCodec(IComplexCodec codec, TypeVisitorContext context)
    {
        // if theres a concrete type def supplied by the
        // user, return their requested type.
        if (!context.IsDynamicResult && !context.InnerRealType)
            return context.Type;

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
        return context.Type;
    }

    private record TypeVisitorContext
    {
        private readonly Type _type;

        public Type Type
        {
            get => _type;
            init
            {
                // remove the wrapper if ones present
                var temp = value;
                while (IWrapper.TryGetWrapper(temp, out var wrapper))
                {
                    temp = wrapper.GetInnerType(temp);
                }

                _type = temp;
            }
        }

        public bool IsDynamicResult
            => Type == typeof(object);

        public string? Name { get; set; }
        public EdgeDBTypeDeserializeInfo? Deserializer { get; set; }
        public bool InnerRealType { get; set; }

        public int Depth { get; init; }

        public string FormattedDepth
            => "".PadRight(Depth, '>') + (Depth > 0 ? " " : "");

        public TypeVisitorContext(Type type)
        {
            // remove the wrapper if ones present
            var temp = type;
            while (IWrapper.TryGetWrapper(temp, out var wrapper))
            {
                temp = wrapper.GetInnerType(temp);
            }

            _type = temp;
        }
    }

}
