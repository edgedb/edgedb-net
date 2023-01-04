using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema
{   
    /// <summary>
    ///     Represents a class responsible for preforming and caching schema introspection data.
    /// </summary>
    internal class SchemaIntrospector
    {
        /// <summary>
        ///     The cache of schema info key'd by the client.
        /// </summary>
        private static readonly ConcurrentDictionary<IEdgeDBQueryable, SchemaInfo> _schemas;

        /// <summary>
        ///     Initializes the schema info collection.
        /// </summary>
        static SchemaIntrospector()
        {
            _schemas = new ConcurrentDictionary<IEdgeDBQueryable, SchemaInfo>();
        }

        /// <summary>
        ///     Gets or creates schema introspection info.
        /// </summary>
        /// <param name="edgedb">The client to preform introspection with if the cache doesn't have it.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync introspection operation. The result of the 
        ///     task is the introspection info.
        /// </returns>
        public static ValueTask<SchemaInfo> GetOrCreateSchemaIntrospectionAsync(IEdgeDBQueryable edgedb, CancellationToken token = default)
        {
            if (_schemas.TryGetValue(edgedb, out var info))
                return ValueTask.FromResult(info);
            return new ValueTask<SchemaInfo>(IntrospectSchemaAsync(edgedb, token));
        }

        /// <summary>
        ///     Preforms an introspection and adds its result to the <see cref="_schemas"/> collection.
        /// </summary>
        /// <param name="edgedb">The client to preform introspection with.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync introspection operation. The result of the 
        ///     task is the introspection info.
        /// </returns>
        private static async Task<SchemaInfo> IntrospectSchemaAsync(IEdgeDBQueryable edgedb, CancellationToken token)
        {
            // select out all object types and filter where they're not built-in
            var result = await QueryBuilder.Select<ObjectType>(shape =>
            {
                shape.IncludeMultiLink(x => x.Constraints);
                shape.IncludeMultiLink(x => x.Properties, shape =>
                    shape.Computeds((ctx, prop) => new
                    {
                        Cardinality = (string)ctx.UnsafeLocal<object>("cardinality") == "One"
                            ? ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.One : DataTypes.Cardinality.AtMostOne
                            : ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.AtLeastOne : DataTypes.Cardinality.Many,
                        TargetId = ctx.UnsafeLocal<Guid>("target.id"),
                        IsLink = ctx.Raw<object>("[IS schema::Link]") != null,
                        IsExclusive = ctx.Raw<bool>("exists (select .constraints filter .name = 'std::exclusive')"),
                        IsComputed = EdgeQL.Length(ctx.UnsafeLocal<object>("computed_fields")) != 0,
                        IsReadonly = ctx.UnsafeLocal<bool>("readonly"),
                        HasDefault = ctx.Raw<bool>("EXISTS .default or (\"std::sequence\" in .target[IS schema::ScalarType].ancestors.name)")
                    })
                );
            }).Filter((x, ctx) => !ctx.UnsafeLocal<bool>("builtin")).ExecuteAsync(edgedb, token: token);
            //var result = await QueryBuilder.Select<ObjectType>(ctx => new ObjectType
            //{
            //    Id = ctx.Include<Guid>(),
            //    IsAbstract = ctx.Include<bool>(),
            //    Name = ctx.Include<string>(),
            //    Constraints = ctx.IncludeMultiLink(() => new Constraint 
            //    {
            //        SubjectExpression = ctx.Include<string>(),
            //        Name = ctx.Include<string>(),
            //    }),
            //    Properties = ctx.IncludeMultiLink(() => new Property
            //    {
            //        Required = ctx.Include<bool>(),
            //        Cardinality = (string)ctx.UnsafeLocal<object>("cardinality") == "One"
            //            ? ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.One : DataTypes.Cardinality.AtMostOne
            //            : ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.AtLeastOne : DataTypes.Cardinality.Many,
            //        Name = ctx.Include<string>(),
            //        TargetId = ctx.UnsafeLocal<Guid>("target.id"),
            //        IsLink = ctx.Raw<object>("[IS schema::Link]") != null,
            //        IsExclusive = ctx.Raw<bool>("exists (select .constraints filter .name = 'std::exclusive')"),
            //        IsComputed = EdgeQL.Length(ctx.UnsafeLocal<object>("computed_fields")) != 0,
            //        IsReadonly = ctx.UnsafeLocal<bool>("readonly"),
            //        HasDefault = ctx.Raw<bool>("EXISTS .default or (\"std::sequence\" in .target[IS schema::ScalarType].ancestors.name)")
            //    })
            //}).Filter((x, ctx) => !ctx.UnsafeLocal<bool>("builtin")).ExecuteAsync(edgedb, token: token);
            
            // add to our cache
            return _schemas[edgedb] = new SchemaInfo(result);
        }
    }
}
