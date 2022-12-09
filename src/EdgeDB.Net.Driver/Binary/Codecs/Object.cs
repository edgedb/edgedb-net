using EdgeDB.Binary;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class Object : ICodec<object>, IArgumentCodec<object>, IMultiWrappingCodec
    {
        private readonly ICodec[] _innerCodecs;
        private readonly string[] _propertyNames;
        private TypeDeserializerFactory? _factory;
        private EdgeDBTypeDeserializeInfo? _deserializerInfo;
        internal TypeIntrospectionDescriptor? TypeAnnotation;
        internal Type? TargetType;
        internal bool Initialized;
        
        internal Object(ObjectShapeDescriptor descriptor, List<ICodec> codecs)
        {
            _innerCodecs = new ICodec[descriptor.Shapes.Length];
            _propertyNames = new string[descriptor.Shapes.Length];

            for(int i = 0; i != descriptor.Shapes.Length; i++)
            {
                var shape = descriptor.Shapes[i];
                _innerCodecs[i] = codecs[shape.TypePos];
                _propertyNames[i] = shape.Name;
            }
        }

        internal Object(NamedTupleTypeDescriptor descriptor, List<ICodec> codecs)
        {
            _innerCodecs = new ICodec[descriptor.Elements.Length];
            _propertyNames = new string[descriptor.Elements.Length];

            for (int i = 0; i != descriptor.Elements.Length; i++)
            {
                var shape = descriptor.Elements[i];
                _innerCodecs[i] = codecs[shape.TypePos];
                _propertyNames[i] = shape.Name;
            }
        }

        internal Object(ICodec[] innerCodecs, string[] propertyNames)
        {
            _innerCodecs = innerCodecs;
            _propertyNames = propertyNames;
        }

        public void Initialize(Type target)
        {
            if (Initialized && target == TargetType)
                return;

            TargetType = target;

            try
            {
                _factory = TypeBuilder.GetDeserializationFactory(target);
                _deserializerInfo = TypeBuilder.TypeInfo[target];
                Initialized = true;
            }
            catch (Exception) when (TargetType == typeof(object))
            {
                _factory = (ref ObjectEnumerator enumerator) => enumerator.ToDynamic();
            }
        }

        public unsafe object? Deserialize(ref PacketReader reader)
        {
            if (!Initialized || _factory is null || TargetType is null)
                Initialize(typeof(object));
            
            // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
            // so we need to pass the underlying data as a reference and wrap a new reader ontop.
            // This method ensures we're not copying the packet in memory again but the downside is
            // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
            var enumerator = new ObjectEnumerator(ref reader.Data, reader.Position, _propertyNames, _innerCodecs, _deserializerInfo);
            
            try
            {
                return _factory!(ref enumerator);
            }
            catch(Exception x)
            {
                throw new EdgeDBException($"Failed to deserialize object to {TargetType}", x);
            }
            finally
            {
                // set the readers position to the enumerators' readers position.
                reader.Position = enumerator.Reader.Position;
            }
        }

        public void Serialize(ref PacketWriter writer, object? value)
        {
            throw new NotImplementedException();
        }

        public void SerializeArguments(ref PacketWriter writer, object? value)
        {
            object?[]? values = null;

            if (value is IDictionary<string, object?> dict)
                values = _propertyNames.Select(x => dict[x]).ToArray();
            else if (value is object?[] arr)
                value = arr;

            if (values is null)
            {
                throw new ArgumentException($"Expected dynamic object or array but got {value?.GetType()?.Name ?? "null"}");
            }

            writer.Write(values.Length);

            for (int i = 0; i != values.Length; i++)
            {
                var element = values[i];
                var innerCodec = _innerCodecs[i];

                // reserved
                writer.Write(0);

                // encode
                if (element is null)
                {
                    writer.Write(-1);
                }
                else
                {
                    // special case for enums
                    if (element.GetType().IsEnum && innerCodec is Text)
                        element = element.ToString();

                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => innerCodec.Serialize(ref innerWriter, element));
                }
            }
        }

        ICodec[] IMultiWrappingCodec.InnerCodecs => _innerCodecs;
    }
}
