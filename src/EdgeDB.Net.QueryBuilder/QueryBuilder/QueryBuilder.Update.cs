using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static partial class QueryBuilder
    {
        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{TType, TType}}, bool)"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update(updateFunc, returnUpdatedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{TType, TType}})"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<TType, TType>> updateFunc)
            => new QueryBuilder<TType>().Update(updateFunc, false);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType, TType}}, bool)"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<QueryContext<TType>, TType, TType>> updateFunc, bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update(updateFunc, returnUpdatedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType, TType}})"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(Expression<Func<QueryContext<TType>, TType, TType>> updateFunc)
            => new QueryBuilder<TType>().Update(updateFunc, false);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType}}, Expression{Func{QueryContext, TType, TType}}, bool)"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(
            Expression<Func<QueryContext<TType>, TType>> selector,
            Expression<Func<QueryContext<TType>, TType, TType>> updateFunc,
            bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update(selector, updateFunc, returnUpdatedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType}}, Expression{Func{TType, TType}}, bool)"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(
            Expression<Func<QueryContext<TType>, TType>> selector,
            Expression<Func<TType, TType>> updateFunc,
            bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update(selector, updateFunc, returnUpdatedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType}}, Expression{Func{TType, TType}})"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(
            Expression<Func<QueryContext<TType>, TType>> selector,
            Expression<Func<TType, TType>> updateFunc)
            => new QueryBuilder<TType>().Update(selector, updateFunc);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Update(Expression{Func{QueryContext, TType}}, Expression{Func{QueryContext, TType, TType}})"/>
        public static IUpdateQuery<TType, QueryContext<TType>> Update<TType>(
            Expression<Func<QueryContext<TType>, TType>> selector,
            Expression<Func<QueryContext<TType>, TType, TType>> updateFunc)
            => new QueryBuilder<TType>().Update(selector, updateFunc);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue)
            => Update(updateFunc, null, returnUpdatedValue);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType, TType>> updateFunc, bool returnUpdatedValue)
            => Update(updateFunc, null, returnUpdatedValue);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TType, TType>> updateFunc)
             => Update(updateFunc, null, false);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType, TType>> updateFunc)
            => Update(updateFunc, null, false);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType>> selector, Expression<Func<TContext, TType, TType>> updateFunc, bool returnUpdatedValue)
            => Update(updateFunc, selector, returnUpdatedValue);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType>> selector, Expression<Func<TType, TType>> updateFunc, bool returnUpdatedValue)
            => Update(updateFunc, selector, returnUpdatedValue);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType>> selector, Expression<Func<TContext, TType, TType>> updateFunc)
            => Update(updateFunc, selector, false);

        /// <inheritdoc/>
        public IUpdateQuery<TType, TContext> Update(Expression<Func<TContext, TType>> selector, Expression<Func<TType, TType>> updateFunc)
            => Update(updateFunc, selector, false);

        /// <summary>
        ///     Adds a generic update node, with the specified update function and target selector.
        /// </summary>
        /// <param name="updateFunc">
        ///     The callback used to update <typeparamref name="TType"/>.
        /// </param>
        /// <param name="selector">
        ///     The expression that selects the object to update.
        /// </param>
        /// <param name="returnUpdatedValue">
        ///     Whether or not to implicitly add a <c>SELECT</c> node that selects the result of the update,
        ///     with a default shape.
        /// </param>
        /// <returns>A <see cref="IUpdateQuery{TType, TContext}"/>.</returns>
        private IUpdateQuery<TType, TContext> Update(LambdaExpression updateFunc, LambdaExpression? selector, bool returnUpdatedValue)
        {
            var updateNode = AddNode<UpdateNode>(new UpdateContext(typeof(TType))
            {
                UpdateExpression = updateFunc,
                Selector = selector,
            });

            if (returnUpdatedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, updateNode);
            }

            return this;
        }

        IMultiCardinalityExecutable<TType> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);
        IMultiCardinalityExecutable<TType> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
    }
}
