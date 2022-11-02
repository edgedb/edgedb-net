using EdgeDB.Binary;
using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     A wrapper for the <see cref="QueryBuilder{TType}"/> which expose basic CRUD methods.
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public sealed class QueryableCollection<TType>
    {
        /// <summary>
        ///     Gets a query builder for <typeparamref name="TType"/>.
        /// </summary>
        public QueryBuilder<TType> QueryBuilder
            => new();

        /// <summary>
        ///     The client used for introspection and execution.
        /// </summary>
        private readonly IEdgeDBQueryable _edgedb;

        /// <summary>
        ///     Constructs a new <see cref="QueryableCollection{TType}"/>.
        /// </summary>
        /// <param name="edgedb">The client to introspect and execute with.</param>
        internal QueryableCollection(IEdgeDBQueryable edgedb)
        {
            _edgedb = edgedb;
        }

        /// <summary>
        ///     Adds a value to <typeparamref name="TType"/>s' collection.
        /// </summary>
        /// <param name="item">The value to add.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <exception cref="EdgeDBErrorException">The item to add violates an exclusive constraint.</exception>
        /// <returns>The added value.</returns>
        public Task<TType?> AddAsync(TType item, CancellationToken token = default)
            => QueryBuilder.Insert(item).ExecuteAsync(_edgedb, token: token);

        /// <summary>
        ///     Adds or updates an existing value based on the types unique constraints.
        /// </summary>
        /// <remarks>
        ///     This method requires introspection and can preform more than one query 
        ///     to cache the current clients schema.
        /// </remarks>
        /// <param name="item">The value to add</param>
        /// <param name="updateFactory">The factory to update the item.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>The added value.</returns>
        public Task<TType?> AddOrUpdateAsync(TType item, Expression<Func<TType, TType>> updateFactory, CancellationToken token = default)
            => QueryBuilder
                .Insert(item)
                .UnlessConflict()
                .Else(b => 
                    (Interfaces.ISingleCardinalityExecutable<TType>)b.Update(updateFactory)
                )
                .ExecuteAsync(_edgedb, token: token);

        /// <summary>
        ///     Adds or updates an existing value based on the types unique constraints.
        ///     If a conflict is met, this function will generate a default update factory that
        ///     updates non-readonly and non-exclusive properties.
        /// </summary>
        /// <remarks>
        ///     This method requires introspection and can preform more than one query 
        ///     to cache the current clients schema.
        /// </remarks>
        /// <param name="item">The value to add and use to update any existing values.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>The added or updated value.</returns>
        public async Task<TType?> AddOrUpdateAsync(TType item, CancellationToken token = default)
        {
            var updateFactory = await QueryGenerationUtils.GenerateUpdateFactoryAsync(_edgedb, item, token).ConfigureAwait(false);

            return await QueryBuilder
                .Insert(item)
                .UnlessConflict()
                .Else(q => 
                    (ISingleCardinalityExecutable<TType>)q.Update(updateFactory!, false)
                ).ExecuteAsync(_edgedb, token: token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Attempts to add a value to this collection.
        /// </summary>
        /// <remarks>
        ///     This method requires introspection and can preform more than one query 
        ///     to cache the current clients schema.
        /// </remarks>
        /// <param name="item">The value to add.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>
        ///     <see langword="true"/> if the value was added successfully, otherwise <see langword="false"/>.
        /// </returns>
        public async Task<bool> TryAddAsync(TType item, CancellationToken token = default)
        {
            var query = QueryBuilder.Insert(item, false).UnlessConflict();
            var result = await query.ExecuteAsync(_edgedb, token: token);
            return result != null;
        }

        /// <summary>
        ///     Gets or adds a value to the collection.
        /// </summary>
        /// <remarks>
        ///     This method requires introspection and can preform more than one query 
        ///     to cache the current clients schema.
        ///     The method will attempt to insert the value if it does not exist 
        ///     based on any properties with exclusive constraints, if a conflict is met 
        ///     then the conflicting object will be returned.
        /// </remarks>
        /// <param name="item">The item to get or add.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>The inserted or conflicting item.</returns>
        public Task<TType> GetOrAddAsync(TType item, CancellationToken token = default)
            => QueryBuilder
                .Insert(item, true)
                .UnlessConflict()
                .ElseReturn()
                .ExecuteAsync(_edgedb, token: token)!;

        /// <summary>
        ///     Deletes a value from the collection.
        /// </summary>
        /// <remarks>
        ///     This method may require introspection of the schema to 
        ///     determine the filter for the delete statement.
        /// </remarks>
        /// <param name="item">The value to delete.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>Whether or not the value was deleted.</returns>
        /// <exception cref="NotSupportedException">No unique constraints found to generate filter condition.</exception>
        public async Task<bool> DeleteAsync(TType item, CancellationToken token = default)
        {
            // try get the objects id
            if (QueryObjectManager.TryGetObjectId(item, out var id))
                return (
                    await QueryBuilder
                        .Delete
                        .Filter((_, ctx) => ctx.UnsafeLocal<Guid>("id") == id)
                        .ExecuteAsync(_edgedb, token: token).ConfigureAwait(false)
                    ).Any();
            
            // try to get exclusive property set on the instance
            var props = await QueryGenerationUtils.GetPropertiesAsync<TType>(_edgedb, exclusive: true, token: token).ConfigureAwait(false);

            if (!props.Any())
                throw new NotSupportedException("No unique constraints found to generate filter condition.");

            // remove non defaults
            props = props.Where(x => ReflectionUtils.GetDefault(x.PropertyType) != x.GetValue(item));


            Dictionary<PropertyInfo, string> variables = new();
            // generate the expression
            var expr = Expression.Lambda<Func<TType, QueryContext<TType>, bool>>(props.Select(x =>
            {
                var name = QueryUtils.GenerateRandomVariableName();
                var typeCast = PacketSerializer.GetEdgeQLType(x.PropertyType);
                var e = Expression.Equal(
                     Expression.MakeMemberAccess(Expression.Parameter(typeof(TType), "x"), x),
                     Expression.Constant($"<{typeCast}>{name}")
                 );

                variables[x] = name;

                return e;
            }
            ).Aggregate((x, y) => Expression.And(x, y)), Expression.Parameter(typeof(TType), "x"), Expression.Parameter(typeof(QueryContext<TType>), "ctx"));

            var builder = QueryBuilder;
            foreach (var (prop, name) in variables)
                builder.AddQueryVariable(name, prop.GetValue(item));
            return (await builder.Delete.Filter(expr).ExecuteAsync(_edgedb, token: token).ConfigureAwait(false)).Any();
        }

        /// <summary>
        ///     Deletes all values that match a given predicate.
        /// </summary>
        /// <param name="filter">The predicate which will determine if a value will be deleted.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous insert operation.</param>
        /// <returns>The number of values deleted.</returns>
        public Task<long> DeleteWhereAsync(Expression<Func<TType, bool>> filter, CancellationToken token = default)
            => ((ISingleCardinalityExecutable<long>)QueryBuilder.Select(() => EdgeQL.Count(QueryBuilder.Delete.Filter(filter)))).ExecuteAsync(_edgedb, token: token);

        /// <summary>
        ///     Filters the current collection by a predicate.
        /// </summary>
        /// <param name="filter">The predicate to filter by.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous select operation.</param>
        /// <returns>A collection of <typeparamref name="TType"/> that match the provided predicate.</returns>
        public async Task<IReadOnlyCollection<TType?>> WhereAsync(Expression<Func<TType, bool>> filter, 
            CancellationToken token = default)
            => await QueryBuilder.Select().Filter(filter).ExecuteAsync(_edgedb, token: token);

        /// <summary>
        ///     Updates a given value in the collection with an update factory.
        /// </summary>
        /// <remarks>
        ///     This method may require introspection of the schema to 
        ///     generate the filter for the update statement.
        /// </remarks>
        /// <param name="value">The value to update.</param>
        /// <param name="updateFunc">The factory containing the updated version.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous select operation.</param>
        /// <returns>The updated item.</returns>
        public async Task<TType?> UpdateAsync(TType value, Expression<Func<TType, TType>> updateFunc, CancellationToken token = default)
            => (await QueryBuilder
                .Update(updateFunc)
                .Filter(await QueryGenerationUtils.GenerateUpdateFilterAsync(_edgedb, value, token))
                .ExecuteAsync(_edgedb, token: token).ConfigureAwait(false)).FirstOrDefault();

        /// <summary>
        ///     Updates a given value in the collection.
        /// </summary>
        /// <remarks>
        ///     This method may require introspection of the schema to 
        ///     generate the filter and update factory for the update statement.
        /// </remarks>
        /// <param name="value">The value to update.</param>
        /// <param name="token">A cancellation token to cancel the asynchronous select operation.</param>
        /// <returns>The updated item.</returns>
        public async Task<TType?> UpdateAsync(TType value, CancellationToken token = default)
            => (await QueryBuilder
                .Update(await QueryGenerationUtils.GenerateUpdateFactoryAsync(_edgedb, value, token))
                .Filter(await QueryGenerationUtils.GenerateUpdateFilterAsync(_edgedb, value, token))
                .ExecuteAsync(_edgedb, token: token).ConfigureAwait(false)).FirstOrDefault();
    }
}
