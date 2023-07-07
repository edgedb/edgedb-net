using EdgeDB.Binary.Codecs;
using EdgeDB.DataTypes;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace EdgeDB
{
    internal sealed class ObjectBuilder
    {
        private static readonly Dictionary<Type, (int Version, ICodec Codec)> _codecVisitorStateTable = new();
        private static readonly object _visitorLock = new();

        public static TType? BuildResult<TType>(EdgeDBBinaryClient client, ICodec codec, in ReadOnlyMemory<byte> data)
        {
            // TO INVESTIGATE: since a codec can only be "visited" or "mutated" for
            // one type at a time, we have to ensure that the codec is ready to deserialize
            // TType, we can store the states of the codecs here for building result
            // to achieve this.
            var typeCodec = codec;

            bool wasSkipped = false;
            lock(_visitorLock)
            {
                // Since the supplied codec could be a template, we walk the codec and cache based on the supplied type
                // if the codec hasn't been walked OR the result type is an object we walk it and cache it if its not an
                // object codec.
                if (typeof(TType) != typeof(object) && _codecVisitorStateTable.TryGetValue(typeof(TType), out var info))
                {
                    if(codec.GetHashCode() == info.Version)
                    {
                        typeCodec = info.Codec;
                        wasSkipped = true;
                    }
                }
                else
                {
                    var version = codec.GetHashCode();

                    var visitor = new TypeVisitor(client);

                    visitor.SetTargetType(typeof(TType));

                    visitor.Visit(ref codec);

                    if (typeof(TType) != typeof(object))
                    _codecVisitorStateTable[typeof(TType)] = (version, codec);

                    typeCodec = codec;
                }
            }

            if(wasSkipped)
            {
                client.Logger.SkippingCodecVisiting(typeof(TType), codec);
            }
            

            if (typeCodec is ObjectCodec objectCodec)
            {
                return (TType?)TypeBuilder.BuildObject(client, typeof(TType), objectCodec, in data);
            }

            var value = typeCodec.Deserialize(client, in data);

            return (TType?)ConvertTo(typeof(TType), value);
        }

        public static object? ConvertTo(Type type, object? value)
        {
            if (value is null)
            {
                return ReflectionUtils.GetDefault(type);
            }

            var valueType = value.GetType();

            if (valueType.IsAssignableTo(type))
                return value;

            // check for nullable
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // if the value was null, the above IsAssignableTo check would have returned true,
                // return a convert with the inner generic
                return ConvertTo(type.GenericTypeArguments[0], value);
            }

            // check for enums
            if(value is string str && type.IsEnum)
            {
                return Enum.Parse(type, str);
            }

            // check for arrays or sets
            if ((valueType.IsArray || valueType.IsAssignableTo(typeof(IEnumerable))) && (type.IsArray || type.IsAssignableFrom(typeof(IEnumerable)) || type.IsAssignableTo(typeof(IEnumerable))))
            {
                return ConvertCollection(type, valueType, value);
            }
            
            // check for edgeql types
            //if (TypeBuilder.IsValidObjectType(type) && value is IDictionary<string, object?> dict)
            //    return TypeBuilder.BuildObject(type, dict);

            // check for tuple
            if(value is TransientTuple tuple && type.GetInterface("ITuple") != null)
            {
                if (type.Name.StartsWith("ValueTuple"))
                    return tuple.ToValueTuple();
                else
                    return tuple.ToReferenceTuple();
            }

            // check for F# option
            if(type.IsFSharpOption())
            {
                // convert inner value
                var innerValue = ConvertTo(type.GenericTypeArguments[0], value);
                return Activator.CreateInstance(type, new object?[] { innerValue });
            }

            if (type.IsFSharpValueOption())
            {
                // is the value null?
                if (value is null)
                {
                    return type.GetProperty("ValueNone", BindingFlags.Static | BindingFlags.Public)!.GetValue(null);
                }

                var newValueSome = type.GetMethod("NewValueSome", BindingFlags.Static | BindingFlags.Public)!;
                var innerValue = ConvertTo(type.GenericTypeArguments[0], value);
                return newValueSome.Invoke(null, new object?[] { innerValue });
            }

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch
            {
                try
                {
                    return ReflectionUtils.DynamicCast(value, type);
                }
                catch
                {
                    throw new ArgumentException($"Cannot convert {valueType} to type {type}");
                }
            }
        }

        internal static object? ConvertCollection(Type targetType, Type valueType, object value)
        {
            List<object?> converted = new();
            var strongInnerType = targetType.IsArray ? targetType.GetElementType()! : targetType.GenericTypeArguments.FirstOrDefault();

            foreach (var val in (IEnumerable)value)
            {
                converted.Add(strongInnerType is not null ? ConvertTo(strongInnerType, val) : val);
                
                //if (val is IDictionary<string, object?> raw)
                //{
                //    converted.Add(strongInnerType is not null ? TypeBuilder.BuildObject(strongInnerType, raw) : val);
                //}
                //else
                    

            }

            var arr = Array.CreateInstance(strongInnerType ?? valueType.GenericTypeArguments[0], converted.Count);
            Array.Copy(converted.ToArray(), arr, converted.Count);

            switch (targetType)
            {
                case Type when targetType.Name == typeof(List<>).Name:
                    {
                        var l = typeof(List<>).MakeGenericType(strongInnerType ?? valueType.GenericTypeArguments[0]);
                        return Activator.CreateInstance(l, arr);
                    }
                case Type when targetType.IsArray || targetType.IsAssignableTo(typeof(IEnumerable)):
                    {
                        return arr;
                    }
                default:
                    {
                        if (arr.GetType().IsAssignableTo(targetType))
                            return ReflectionUtils.DynamicCast(arr, targetType);

                        throw new EdgeDBException($"Couldn't convert {valueType} to {targetType}");
                    }
            }
        }
    }
}
