using System;
using System.Xml.Linq;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TypeVisitor : CodecVisitor
    {
        private TypeResultFrame Context
            => _frames.Peek();

        private readonly Stack<TypeResultFrame> _frames;

        public TypeVisitor()
        {
            _frames = new();
        }

        public void SetTargetType(Type type)
        {
            EdgeDBTypeDeserializeInfo? deserializer = null;

            if (type != typeof(void))
            {
                _ = TypeBuilder.IsValidObjectType(type) && TypeBuilder.TryGetTypeDeserializerInfo(type, out deserializer);
            }

            _frames.Push(new() { Type = type, Deserializer = deserializer });
        }

        public void Reset()
            => _frames.Clear();

        protected override void VisitCodec(ref ICodec codec)
        {
            if (Context.Type == typeof(void))
                return;

            switch (codec)
            {
                case ObjectCodec obj:
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
                case TupleCodec tuple:
                    {
                        var tupleTypes = DataTypes.TransientTuple.FlattenTypes(Context.Type);

                        if (tupleTypes.Length != tuple.InnerCodecs.Length)
                            throw new NoTypeConverterException($"Cannot determine inner types of the tuple {Context.Type}");

                        for (int i = 0; i != tuple.InnerCodecs.Length; i++)
                        {
                            var innerCodec = tuple.InnerCodecs[i];
                            var type = tupleTypes[i];
                            using var elementHandle = EnterNewContext(type);

                            Visit(ref innerCodec);

                            tuple.InnerCodecs[i] = innerCodec;
                        }
                    }
                    break;
                case CompilableWrappingCodec compilable:
                    {
                        // visit the inner codec
                        var tmp = compilable.InnerCodec;

                        using (var innerHandle = EnterNewContext(Context.Type.GetWrappingType()))
                        {
                            VisitCodec(ref tmp);

                            codec = compilable.Compile(tmp);
                        }

                        // visit the compiled codec
                        Visit(ref codec);
                    }
                    break;
                case IComplexCodec complex:
                    {
                        codec = complex.GetCodecFor(Context.Type);
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
                        codec = runtime.Broker.GetCodecFor(Context.Type);
                    }
                    break;
            }
        }

        private IDisposable EnterNewContext(
            Type type,
            string? name = null,
            EdgeDBTypeDeserializeInfo? deserializer = null)
        {
            _frames.Push(new TypeResultFrame { Type = type, Name = name, Deserializer = deserializer });
            return new FrameHandle(_frames);
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

