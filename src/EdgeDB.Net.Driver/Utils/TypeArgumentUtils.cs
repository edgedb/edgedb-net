using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EdgeDB;

internal static class TypeArgumentUtils
{
    private static readonly MethodInfo _dictAddMethod =
        typeof(Dictionary<string, object?>).GetMethod(nameof(Dictionary<string, object?>.Add))!;

    private static readonly ConcurrentDictionary<Type, ITypeArgumentBuilder> _builders = new();

    public static bool IsValidArgumentType(Type type)
        => _builders.ContainsKey(type) || type.IsAnonymousType();

    [return: NotNullIfNotNull(nameof(value))]
    public static IDictionary<string, object?>? CreateArguments(Type type, object? value)
    {
        if (value is IDictionary<string, object?> dict)
            return dict;

        if (!IsValidArgumentType(type))
            throw new NotSupportedException($"The type {type} is not a valid argument type");

        return _builders.GetOrAdd(type, t =>
            (ITypeArgumentBuilder)Activator.CreateInstance(typeof(TypeArgumentBuilder<>).MakeGenericType(type))!
        ).CreateArgs(value);
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static IDictionary<string, object?>? CreateArguments<T>(T? value)
        => CreateArguments(typeof(T), value);

    private interface ITypeArgumentBuilder
    {
        [return: NotNullIfNotNull(nameof(value))]
        IDictionary<string, object?>? CreateArgs(object? value);
    }

    private sealed class TypeArgumentBuilder<T> : ITypeArgumentBuilder
    {
        private readonly Func<T, IDictionary<string, object?>> _factory;
        private readonly Type _type;

        public TypeArgumentBuilder()
        {
            _type = typeof(T);

            if (RuntimeFeature.IsDynamicCodeCompiled)
                _factory = CompileExpressionBuilder(_type);
            else
                _factory = GetReflectionBuilder(_type);
        }

        IDictionary<string, object?>? ITypeArgumentBuilder.CreateArgs(object? value)
        {
            if (value is null)
                return null;

            if (value is not T tvalue)
                throw new ArgumentException($"value is not of type {_type}", nameof(value));

            return CreateArgs(tvalue);
        }

        private static Func<T, IDictionary<string, object?>> GetReflectionBuilder(Type type)
        {
            var propMap = EdgeDBPropertyMapInfo.Create(type);

            return value =>
            {
                var dict = new Dictionary<string, object?>();

                foreach (var prop in propMap.Map)
                {
                    dict.Add(prop.Key, prop.Value.PropertyInfo.GetValue(value));
                }

                return dict;
            };
        }

        private static Func<T, IDictionary<string, object?>> CompileExpressionBuilder(Type type)
        {
            var param = Expression.Parameter(type, "value");
            var dictExp = Expression.Variable(typeof(Dictionary<string, object?>), "dict");

            var body = new List<Expression>
            {
                Expression.Assign(dictExp, Expression.New(typeof(Dictionary<string, object?>)))
            };

            var propMap = EdgeDBPropertyMapInfo.Create(type);

            foreach (var prop in propMap.Map)
            {
                Expression value = prop.Value.Type.IsValueType
                    ? Expression.TypeAs(Expression.MakeMemberAccess(param, prop.Value.PropertyInfo), typeof(object))
                    : Expression.MakeMemberAccess(param, prop.Value.PropertyInfo);

                body.Add(
                    Expression.Call(
                        dictExp,
                        _dictAddMethod,
                        Expression.Constant(prop.Key),
                        value
                    )
                );
            }

            body.Add(dictExp);

            return Expression.Lambda<Func<T, IDictionary<string, object?>>>(
                Expression.Block(
                    typeof(IDictionary<string, object?>),
                    new[] {dictExp},
                    body
                ),
                param
            ).Compile();
        }

        [return: NotNullIfNotNull(nameof(value))]
        public IDictionary<string, object?>? CreateArgs(T value)
            => _factory(value);
    }
}
