using EdgeDB.Binary.Codecs;
using EdgeDB.DataTypes;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace EdgeDB;

internal sealed class ObjectBuilder
{
    private static readonly ConcurrentDictionary<Type, (int Version, ICodec Codec)> _codecVisitorStateTable = new();
    private static readonly object _visitorLock = new();

    public static PreheatedCodec PreheatCodec<T>(EdgeDBBinaryClient client, ICodec codec)
    {
        // if the codec has been visited before and we have the most up-to-date version, return it.
        if (
            typeof(T) != typeof(object) &&
            _codecVisitorStateTable.TryGetValue(typeof(T), out var info) &&
            codec.GetHashCode() == info.Version)
        {
            client.Logger.SkippingCodecVisiting(typeof(T), codec);
            return new PreheatedCodec(info.Codec);
        }

        var version = codec.GetHashCode();

        var visitor = new TypeVisitor(client);
        visitor.SetTargetType(typeof(T));
        visitor.Visit(ref codec);

        if (typeof(T) != typeof(object))
            _codecVisitorStateTable[typeof(T)] = (version, codec);

        if (client.Logger.IsEnabled(LogLevel.Debug))
        {
            client.Logger.ObjectDeserializationPrep(CodecFormatter.Format(codec).ToString());
        }

        return new PreheatedCodec(codec);
    }

    public static T? BuildResult<T>(EdgeDBBinaryClient client, in PreheatedCodec preheated,
        in ReadOnlyMemory<byte> data)
    {
        if (preheated.Codec is ObjectCodec objectCodec)
        {
            return (T?)TypeBuilder.BuildObject(client, typeof(T), objectCodec, in data);
        }

        var value = preheated.Codec.Deserialize(client, in data);

        return (T?)ConvertTo(typeof(T), value);
    }

    public static T? BuildResult<T>(EdgeDBBinaryClient client, ICodec codec, in ReadOnlyMemory<byte> data)
        => BuildResult<T>(client, PreheatCodec<T>(client, codec), data);

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
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // if the value was null, the above IsAssignableTo check would have returned true,
            // return a convert with the inner generic
            return ConvertTo(type.GenericTypeArguments[0], value);
        }

        // check for enums
        if (value is string str && type.IsEnum)
        {
            return Enum.Parse(type, str);
        }

        // check for arrays or sets
        if ((valueType.IsArray || valueType.IsAssignableTo(typeof(IEnumerable))) && (type.IsArray ||
                type.IsAssignableFrom(typeof(IEnumerable)) || type.IsAssignableTo(typeof(IEnumerable))))
        {
            return ConvertCollection(type, valueType, value);
        }

        // check for edgeql types
        //if (TypeBuilder.IsValidObjectType(type) && value is IDictionary<string, object?> dict)
        //    return TypeBuilder.BuildObject(type, dict);

        // check for tuple
        if (value is TransientTuple tuple && type.GetInterface("ITuple") != null)
        {
            if (type.Name.StartsWith("ValueTuple"))
                return tuple.ToValueTuple();
            return tuple.ToReferenceTuple();
        }

        // check for F# option
        if (type.IsFSharpOption())
        {
            // convert inner value
            var innerValue = ConvertTo(type.GenericTypeArguments[0], value);
            return Activator.CreateInstance(type, innerValue);
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
            return newValueSome.Invoke(null, new[] {innerValue});
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
        var strongInnerType = targetType.IsArray
            ? targetType.GetElementType()!
            : targetType.GenericTypeArguments.FirstOrDefault();

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

    public readonly struct PreheatedCodec
    {
        public readonly ICodec Codec;

        public PreheatedCodec(ICodec codec)
        {
            Codec = codec;
        }
    }
}
