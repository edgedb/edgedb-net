using EdgeDB.Binary;
using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.Utils.FSharp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary.Codecs
{
    internal sealed class TypeInitializedObjectCodec : ObjectCodec
    {
        public EdgeDBTypeDeserializeInfo Deserializer
            => _deserializer;

        public ObjectCodec Parent
            => _codec;

        public Type TargetType
            => _targetType;

        private readonly EdgeDBTypeDeserializeInfo _deserializer;
        private readonly Type _targetType;
        private readonly ObjectCodec _codec;

        public TypeInitializedObjectCodec(Type target, ObjectCodec codec)
            : base(codec.Id, codec.Properties, codec.Metadata)
        {
            if (!TypeBuilder.TryGetTypeDeserializerInfo(target, out _deserializer!))
                throw new NoTypeConverterException($"Failed to find type deserializer for {target}");

            _targetType = target;
            _codec = codec;
        }

        public override object? Deserialize(ref PacketReader reader, CodecContext context)
        {
            // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
            // so we need to pass the underlying data as a reference and wrap a new reader ontop.
            // This method ensures we're not copying the packet in memory again but the downside is
            // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
            var enumerator = new ObjectEnumerator(
                in reader.Data,
                reader.Position,
                Properties,
                context
            );

            try
            {
                return _deserializer.Factory(ref enumerator);
            }
            catch (Exception x)
            {
                throw new EdgeDBException($"Failed to deserialize object to {_targetType}", x);
            }
            finally
            {
                // set the readers position to the enumerators' readers position.
                reader.Position = enumerator.Reader.Position;
            }
        }
    }

    internal struct ObjectProperty
    {
        public readonly Cardinality Cardinality;
        public readonly string Name;
        public ICodec Codec;

        public ObjectProperty(Cardinality cardinality, ref ICodec codec, string name)
        {
            Cardinality = cardinality;
            Codec = codec;
            Name = name;
        }
    }

    internal class ObjectCodec
        : BaseArgumentCodec<object>, IMultiWrappingCodec, ICacheableCodec
    {
        public ICodec[] InnerCodecs
            => _codecs;

        public readonly ObjectProperty[] Properties;

        private ConcurrentDictionary<Type, TypeInitializedObjectCodec>? _typeCodecs;

        private ICodec[] _codecs;

        internal ObjectCodec(in Guid id, ObjectProperty[] properties, CodecMetadata? metadata = null)
            : base(in id, metadata)
        {
            Properties = properties;

            _codecs = new ICodec[properties.Length];
            for (var i = 0; i != properties.Length; i++)
                _codecs[i] = properties[i].Codec;
        }

        public TypeInitializedObjectCodec GetOrCreateTypeCodec(Type type)
            => (_typeCodecs ??= new()).GetOrAdd(type, t => new TypeInitializedObjectCodec(t, this));

        public override object? Deserialize(ref PacketReader reader, CodecContext context)
        {   
            // reader is being copied if we just pass it as 'ref reader' to our object enumerator,
            // so we need to pass the underlying data as a reference and wrap a new reader ontop.
            // This method ensures we're not copying the packet in memory again but the downside is
            // our 'reader' variable isn't kept up to data with the reader in the object enumerator.
            var enumerator = new ObjectEnumerator(
                in reader.Data,
                reader.Position,
                Properties,
                context
            );
            
            try
            {
                return enumerator.ToDynamic();
            }
            catch(Exception x)
            {
                throw new EdgeDBException($"Failed to deserialize object", x);
            }
            finally
            {
                // set the readers position to the enumerators' readers position.
                reader.Position = enumerator.Reader.Position;
            }
        }

        public override void SerializeArguments(ref PacketWriter writer, object? value, CodecContext context)
            => Serialize(ref writer, value, context);

        public override void Serialize(ref PacketWriter writer, object? value, CodecContext context)
        {
            object?[]? values = null;

            if (value is IDictionary<string, object?> dict)
                values = Properties.Select(x => dict[x.Name]).ToArray();
            else if (value is object?[] arr)
                value = arr;

            if (values is null)
            {
                throw new ArgumentException($"Expected dynamic object or array but got {value?.GetType()?.Name ?? "null"}");
            }

            writer.Write(values.Length);

            // TODO: maybe cache the visited codecs based on the 'value'.
            var visitor = context.CreateTypeVisitor();

            for (int i = 0; i != values.Length; i++)
            {
                var element = values[i];

                // reserved
                writer.Write(0);

                if (FSharpOptionInterop.TryGet(element, out var option))
                {
                    if(!option.HasValue)
                    {
                        writer.Write(-1);
                        continue;
                    }

                    element = option.Value;
                }

                // encode
                if (element is null)
                {
                    writer.Write(-1);
                }
                else
                {
                    var innerCodec = Properties[i].Codec;

                    // special case for enums
                    if (element.GetType().IsEnum && innerCodec is TextCodec)
                        element = element.ToString();
                    else
                    {
                        visitor.SetTargetType(element.GetType());
                        visitor.Visit(ref innerCodec);
                        visitor.Reset();
                    }

                    writer.WriteToWithInt32Length((ref PacketWriter innerWriter) => innerCodec.Serialize(ref innerWriter, element, context));
                }
            }
        }

        public override string ToString()
            => "object";

        ICodec[] IMultiWrappingCodec.InnerCodecs
        {
            get => InnerCodecs;
            set
            {
                if (value.Length != Properties.Length)
                    throw new InvalidOperationException("Array length mismatch");

                _codecs = value;
                for (var i = 0; i != _codecs.Length; i++)
                    Properties[i].Codec = _codecs[i];
            }
        }
    }
}
