using EdgeDB.Schema;
using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class QueryGenerationUtils
    {
        /// <summary>
        ///     Gets a collection of properties based on flags.
        /// </summary>
        /// <typeparam name="TType">The type to get the properties on.</typeparam>
        /// <param name="edgedb">A client to preform introspection with.</param>
        /// <param name="exclusive">
        ///     <see langword="true"/> to return only exclusive properties.
        ///     <see langword="false"/> to exclude exclusive properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="readonly">
        ///     <see langword="true"/> to return only readonly properties.
        ///     <see langword="false"/> to exclude readonly properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a collection of <see cref="PropertyInfo"/>.
        /// </returns>
        public static async ValueTask<IEnumerable<PropertyInfo>> GetPropertiesAsync<TType>(IEdgeDBQueryable edgedb, bool? exclusive = null, bool? @readonly = null, CancellationToken token = default)
        {
            var introspection = await SchemaIntrospector.GetOrCreateSchemaIntrospectionAsync(edgedb, token).ConfigureAwait(false);

            return GetProperties(introspection, typeof(TType), exclusive, @readonly);
        }

        /// <summary>
        ///     Gets a collection of properties based on flags.
        /// </summary>
        /// <param name="schemaInfo">
        ///     The introspection data on which to cross reference property data.
        /// </param>
        /// <param name="type">The type to get the properties on.</param>
        /// <param name="exclusive">
        ///     <see langword="true"/> to return only exclusive properties.
        ///     <see langword="false"/> to exclude exclusive properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="readonly">
        ///     <see langword="true"/> to return only readonly properties.
        ///     <see langword="false"/> to exclude readonly properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="includeId">Whether or not to include the 'id' property.</param>
        /// <returns>A collection of <see cref="PropertyInfo"/>.</returns>
        /// <exception cref="NotSupportedException">
        ///     The given type was not found within the introspection data.
        /// </exception>
        public static IEnumerable<PropertyInfo> GetProperties(SchemaInfo schemaInfo, Type type, bool? exclusive = null, bool? @readonly = null, bool includeId = false)
        {
            if (!schemaInfo.TryGetObjectInfo(type, out var info))
                throw new NotSupportedException($"Cannot use {type.Name} as there is no schema information for it.");

            var props = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);
            return props.Where(x =>
            {
                var edgedbName = x.GetEdgeDBPropertyName();
                if (!includeId && edgedbName == "id")
                    return false;
                return info.Properties!.Any(x => x.Name == edgedbName &&
                    (!exclusive.HasValue || x.IsExclusive == exclusive.Value) &&
                    (!@readonly.HasValue || x.IsReadonly == @readonly.Value));
            });
        }

        public static Dictionary<EdgeDBPropertyInfo, Property> MapProperties(SchemaInfo schemaInfo, Type type)
        {
            var map = EdgeDBPropertyMapInfo.Create(type);

            if (!schemaInfo.TryGetObjectInfo(type, out var info))
                throw new NotSupportedException($"Cannot use {type.Name} as there is no schema information for it.");

            return map.Properties.ToDictionary(x => x,
                x => info.Properties!.First(y => y.Name == x.EdgeDBName)!);
        }

        /// <summary>
        ///     Generates a default insert shape expression for the given type and value.
        /// </summary>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="type">The type to generate the shape for.</param>
        /// <returns>
        ///     An <see cref="Expression"/> that contains the insert shape for the given type.
        /// </returns>
        public static Expression GenerateInsertShapeExpression(object? value, Type type)
        {
            var props = type.GetProperties()
                            .Where(x =>
                                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null &&
                                x.GetValue(value) != ReflectionUtils.GetDefault(x.PropertyType));

            return Expression.MemberInit(
                Expression.New(type),
                props.Select(x =>
                    Expression.Bind(x, Expression.MakeMemberAccess(Expression.Constant(value), x))
                    )
                );
        }

        /// <summary>
        ///     Generates a default update factory expression for the given type and value.
        /// </summary>
        /// <typeparam name="TType">The type to generate the shape for.</typeparam>
        /// <param name="edgedb">A client used to preform introspection with.</param>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a generated update factory expression.
        /// </returns>
        public static async ValueTask<Expression<Func<TType, TType>>> GenerateUpdateFactoryAsync<TType>(IEdgeDBQueryable edgedb, TType value, CancellationToken token = default)
        {
            var props = await GetPropertiesAsync<TType>(edgedb, @readonly: false, token: token).ConfigureAwait(false);

            props = props.Where(x => x.GetValue(value) != ReflectionUtils.GetDefault(x.PropertyType));

            return Expression.Lambda<Func<TType, TType>>(
                Expression.MemberInit(
                    Expression.New(typeof(TType)), props.Select(x =>
                        Expression.Bind(x, Expression.MakeMemberAccess(Expression.Constant(value), x)))
                    ),
                Expression.Parameter(typeof(TType), "x")
            );
        }

        /// <summary>
        ///     Generates a default filter for the given type.
        /// </summary>
        /// <typeparam name="TType">The type to generate the filter for.</typeparam>
        /// <param name="edgedb">A client used to preform introspection with.</param>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a generated filter expression.
        /// </returns>
        public static async ValueTask<Expression<Func<TType, QueryContextSelf<TType>, bool>>> GenerateUpdateFilterAsync<TType>(IEdgeDBQueryable edgedb, TType value, CancellationToken token = default)
        {
            // TODO: revisit references
            // try and get object id
            //if (QueryObjectManager.TryGetObjectId(value, out var id))
            //    return (_, ctx) => ctx.UnsafeLocal<Guid>("id") == id;

            // get exclusive properties.
            var exclusiveProperties = await GetPropertiesAsync<TType>(edgedb, exclusive: true, token: token).ConfigureAwait(false);

            var unsafeLocalMethod = typeof(QueryContextSelf<TType>).GetMethod("UnsafeLocal", genericParameterCount: 0, new Type[] {typeof(string)})!;
            return Expression.Lambda<Func<TType, QueryContextSelf<TType>, bool>>(
                exclusiveProperties.Select(x =>
                {

                    return Expression.Equal(
                        Expression.Call(
                            Expression.Parameter(typeof(QueryContextSelf<TType>), "ctx"),
                            unsafeLocalMethod,
                            Expression.Constant(x.GetEdgeDBPropertyName())
                        ),
                        Expression.MakeMemberAccess(Expression.Constant(value), x)
                    );
                }).Aggregate((x, y) => Expression.And(x, y)),
                Expression.Parameter(typeof(TType), "x"),
                Expression.Parameter(typeof(QueryContextSelf<TType>), "ctx")
            );
        }
    }
}
