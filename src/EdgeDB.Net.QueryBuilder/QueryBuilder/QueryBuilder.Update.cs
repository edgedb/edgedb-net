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
        public static IUpdateQuery<TType, QueryContextSelf<TType>> Update<TType>()
            => new QueryBuilder<TType>().Update<TType>(null, false);

        public static IUpdateQuery<TType, QueryContextSelf<TType>> Update<TType>(bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update<TType>(null, returnUpdatedValue);

        public static IUpdateQuery<TType, QueryContextSelf<TType>> Update<TType>(
            Expression<Func<QueryContext, TType>> selector)
            => new QueryBuilder<TType>().Update<TType>(selector, false);

        public static IUpdateQuery<TType, QueryContextSelf<TType>> Update<TType>(
            Expression<Func<QueryContext, TType>> selector, bool returnUpdatedValue)
            => new QueryBuilder<TType>().Update<TType>(selector, returnUpdatedValue);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        /// <summary>
        ///     Adds a generic update node, with the specified update function and target selector.
        /// </summary>
        /// <param name="selector">
        ///     The expression that selects the object to update.
        /// </param>
        /// <param name="returnUpdatedValue">
        ///     Whether or not to implicitly add a <c>SELECT</c> node that selects the result of the update,
        ///     with a default shape.
        /// </param>
        /// <returns>A <see cref="IUpdateQuery{TType, TContext}"/>.</returns>
        internal IUpdateQuery<TSelected, TContext> Update<TSelected>(LambdaExpression? selector, bool returnUpdatedValue)
        {
            var updateNode = AddNode<UpdateNode>(new UpdateContext(typeof(TType))
            {
                Selector = selector,
            });

            if (returnUpdatedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, updateNode);
            }

            return EnterNewType<TSelected>();
        }

        private IMultiCardinalityExecutable<TType> Set(IUpdateShapeBuilder shape)
        {
            if (CurrentUserNode is not UpdateNode updateNode)
                throw new InvalidOperationException("Cannot 'set' on a node that isn't 'Update'");

            updateNode.Set(shape);

            return this;
        }

        IUpdateQuerySet<TType, TContext> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);

        IUpdateQuerySet<TType, TContext> IUpdateQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);

        IMultiCardinalityExecutable<TType> IUpdateQuerySet<TType, TContext>.Set<TAnon>(
            Expression<Func<TType, TAnon>> shape)
            => Set(UpdateShapeBuilder<TType, TContext>.FromInitExpression(shape));

        IMultiCardinalityExecutable<TType> IUpdateQuerySet<TType, TContext>.Set<TAnon>(Expression<Func<TType, TContext, TAnon>> shape)
            => Set(UpdateShapeBuilder<TType, TContext>.FromInitExpression(shape));

        IMultiCardinalityExecutable<TType> IUpdateQuerySet<TType, TContext>.Set(
            Action<UpdateShapeBuilder<TType, TContext>> shape)
        {
            var builder = new UpdateShapeBuilder<TType, TContext>();
            shape(builder);
            return Set(builder);
        }

        IUpdateQuery<TSelected, TContext> IQueryBuilder<TType, TContext>.Update<TSelected>(
            Expression<Func<TType, TContext, TSelected>> selector, bool returnUpdatedValue)
            => Update<TSelected>(selector, returnUpdatedValue);

        IUpdateQuery<TSelected, TContext> IQueryBuilder<TType, TContext>.Update<TSelected>(
            Expression<Func<TType, TSelected>> selector)
            => Update<TSelected>(selector, false);
    }
}
