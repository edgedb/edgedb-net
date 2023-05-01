using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class TypeArgumentUtils
    {
        private static readonly MethodInfo _dictAddMethod = typeof(Dictionary<string,object?>).GetMethod(nameof(Dictionary<string,object?>.Add))!;

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

                var param = Expression.Parameter(_type, "value");
                var dictExp = Expression.Variable(typeof(Dictionary<string, object?>), "dict");

                var body = new List<Expression>()
                {
                     Expression.Assign(dictExp, Expression.New(typeof(Dictionary<string, object?>)))
                };

                var propMap = EdgeDBPropertyMapInfo.Create(_type);

                foreach(var prop in propMap.Map)
                {
                    body.Add(
                        Expression.Call(
                            dictExp,
                            _dictAddMethod,
                            Expression.Constant(prop.Key),
                            Expression.MakeMemberAccess(param, prop.Value.PropertyInfo)
                        )
                    );
                }

                body.Add(dictExp);

                _factory = Expression.Lambda<Func<T, IDictionary<string, object?>>>(
                    Expression.Block(
                        typeof(IDictionary<string, object?>),
                        new ParameterExpression[] { dictExp },
                        body
                    ),
                    param
                ).Compile();
            }

            [return: NotNullIfNotNull(nameof(value))]
            public IDictionary<string, object?>? CreateArgs(T value)
                => _factory(value);

            IDictionary<string, object?>? ITypeArgumentBuilder.CreateArgs(object? value)
            {
                if (value is null)
                    return null;

                if (value is not T tvalue)
                    throw new ArgumentException($"value is not of type {_type}", nameof(value));

                return CreateArgs(tvalue);
            }
        }

        private static readonly ConcurrentDictionary<Type, ITypeArgumentBuilder> _builders = new();

        public static bool IsValidArgumentType(Type type)
            => _builders.ContainsKey(type) ||
                (
                    type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0 &&
                    type.FullName!.Contains("AnonymousType")
                );

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
    }
}
