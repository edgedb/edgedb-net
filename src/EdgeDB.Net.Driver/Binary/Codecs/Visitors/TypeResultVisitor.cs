using System;
using System.Xml.Linq;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TypeResultVisitor : CodecVisitor
    {
        private readonly bool _isObjectType;
        private TypeResultFrame Context
            => _frames.Peek();

        private readonly Stack<TypeResultFrame> _frames;

        public TypeResultVisitor(Type resultType)
        {
            _frames = new();

            EdgeDBTypeDeserializeInfo? deserializer = null;

            if(resultType != typeof(void))
            {
                _isObjectType = TypeBuilder.IsValidObjectType(resultType);

                _ = _isObjectType && TypeBuilder.TryGetTypeDeserializerInfo(resultType, out deserializer);
            }

            _frames.Push(new() { Type = resultType, Deserializer = deserializer });
        }

        protected override void VisitCodec(ref ICodec codec)
        {
            if (Context.Type == typeof(void))
                return;

            switch (codec)
            {
                case Object obj:
                    {
                        obj.Initialize(Context.Type);

                        if (obj.TargetType is null || obj.DeserializerInfo is null)
                            throw new NullReferenceException("Could not find deserializer info for object codec.");

                        using var objHandle = EnterNewContext(obj.TargetType, obj.TargetType.Name, obj.DeserializerInfo);
                        for (int i = 0; i != obj.InnerCodecs.Length; i++)
                        {
                            var innerCodec = obj.InnerCodecs[i];
                            var name = obj.PropertyNames[i];

                            // use the defined type, if not found, use the codecs type
                            var propType = Context.Deserializer!.PropertyMap.TryGetValue(name, out var propInfo)
                                ? propInfo.Type
                                : innerCodec.ConverterType;

                            using var propHandle = EnterNewContext(propType, name);

                            Visit(ref innerCodec);

                            obj.InnerCodecs[i] = innerCodec;
                        }
                    }
                    break;
                case Tuple tuple:
                    {
                        var tupleTypes = DataTypes.TransientTuple.FlattenTypes(Context.Type);

                        if (tupleTypes.Length != tuple.InnerCodecs.Length)
                            throw new NoTypeConverterException($"Cannot determine inner types of the tuple {Context.Type}");

                        for(int i = 0; i != tuple.InnerCodecs.Length; i++)
                        {
                            var innerCodec = tuple.InnerCodecs[i];
                            var type = tupleTypes[i];
                            using var elementHandle = EnterNewContext(type);

                            Visit(ref innerCodec);

                            tuple.InnerCodecs[i] = innerCodec;
                        }
                    }
                    break;
                default:
                    TryMutateToContext(ref codec);
                    break;
            }
        }

        private bool TryMutateToContext(ref ICodec codec)
        {
            switch (codec)
            {
                case CompilableWrappingCodec compilable:
                    {
                        var tmp = compilable.InnerCodec;

                        using var innerHandle = EnterNewContext(GetWrappingType(Context.Type));

                        VisitCodec(ref tmp);

                        compilable.InnerCodec = tmp;

                        codec = compilable.Compile();
                        return true;
                    }
                case ITemporalCodec temporal:
                    codec = temporal.GetCodecFor(Context.Type);
                    return true;
            }

            return false;
        }

        private IDisposable EnterNewContext(
            Type type,
            string? name = null,
            EdgeDBTypeDeserializeInfo? deserializer = null)
        {
            _frames.Push(new TypeResultFrame { Type = type, Name = name, Deserializer = deserializer });
            return new FrameHandle(_frames);
        }

        private static Type GetWrappingType(Type type)
        {
            if(type.GenericTypeArguments.Length == 1)
            {
                return type.GenericTypeArguments[0];
            }

            Type? iface = null;
            if((iface = type.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable`1")) is not null)
            {
                return iface.GenericTypeArguments[0];
            }

            throw new NotSupportedException($"Cannot find valid wrapping type on type {type}");
        }

        private struct TypeResultFrame
        {
            public readonly Type Type { get; init; }
            public string? Name { get; set; }
            public EdgeDBTypeDeserializeInfo? Deserializer { get; set; }
        }

        private readonly struct FrameHandle : IDisposable
        {
            private readonly Stack<TypeResultFrame> _stack;

            public FrameHandle(Stack<TypeResultFrame> stack)
            {
                _stack = stack;
            }

            public void Dispose()
            {
                _stack.Pop();
            }
        }
    }
}

