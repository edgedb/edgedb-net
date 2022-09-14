using EdgeDB.Binary;
using EdgeDB.Binary.Builders;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary.Codecs
{
    internal class Object : ICodec<object>, IArgumentCodec<object>
    {
        private readonly ICodec[] _innerCodecs;
        private readonly string[] _propertyNames;
        private TypeDeserializerFactory? _factory;
        private Type? _targetType;
        private bool _initialized;
        
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
            if (_initialized)
                return;

            _targetType = target;

            try
            {
                _factory = TypeBuilder.GetDeserializationFactory(target);
                var deserializerInfo = TypeBuilder.TypeInfo[target];
                
                // initialize any other object codecs that are properties
                for (int i = 0; i != _innerCodecs.Length; i++)
                {
                    if (_innerCodecs[i] is Object objCodec)
                    {
                        if (!deserializerInfo.PropertyMap.TryGetValue(_propertyNames[i], out var propInfo))
                            throw new EdgeDBException($"Property {_propertyNames[i]} not found on type {target.Name}");

                        objCodec.Initialize(propInfo.Type);
                    }
                }

                _initialized = true;
            }
            catch (Exception) when (_targetType == typeof(object))
            {
                _factory = (ref ObjectEnumerator enumerator) => enumerator.ToDynamic();
            }
        }

        public unsafe object? Deserialize(ref PacketReader reader)
        {
            if (!_initialized || _factory is null || _targetType is null)
                Initialize(typeof(object));
            
            // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
            // so we need to pass the underlying data as a reference and wrap a new reader ontop.
            // This method ensures we're not copying the packet in memory again but the downside is
            // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
            var enumerator = new ObjectEnumerator(ref reader.Data, reader.Position, _propertyNames, _innerCodecs);
            
            try
            {
                return _factory!(ref enumerator);
            }
            catch(Exception x)
            {
                throw new EdgeDBException($"Failed to deserialize object to {_targetType}", x);
            }
            finally
            {
                // set the readers position to the enumerators' readers position.
                reader.Position = enumerator.Reader.Position;
            }
        }

        public void Serialize(PacketWriter writer, object? value)
        {
            throw new NotImplementedException();
        }

        public void SerializeArguments(PacketWriter writer, object? value)
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

            using var innerWriter = new PacketWriter();
            for (int i = 0; i != values.Length; i++)
            {
                var element = values[i];
                var innerCodec = _innerCodecs[i];

                // reserved
                innerWriter.Write(0);

                // encode
                if (element is null)
                {
                    innerWriter.Write(-1);
                }
                else
                {
                    // special case for enums
                    if (element.GetType().IsEnum && innerCodec is Text)
                        element = element.ToString();

                    var elementBuff = innerCodec.Serialize(element);

                    innerWriter.Write(elementBuff.Length);
                    innerWriter.Write(elementBuff);
                }
            }

            writer.Write((int)innerWriter.BaseStream.Length + 4);
            writer.Write(values.Length);
            writer.Write(innerWriter);
        }
    }
}
