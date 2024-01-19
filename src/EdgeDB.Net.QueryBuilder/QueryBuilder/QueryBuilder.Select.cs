using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static partial class QueryBuilder
    {
        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Select()"/>
        public static ISelectQuery<TType, QueryContext<TType>> Select<TType>()
            => new QueryBuilder<TType>().Select();

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Select{TResult}(Action{ShapeBuilder{TResult}})"/>
        public static ISelectQuery<TType, QueryContext<TType>> Select<TType>(Action<ShapeBuilder<TType>> shape)
            => new QueryBuilder<TType>().Select(shape);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public static ISelectQuery<TType, QueryContext<TType>> SelectExpression<TType>(Expression<Func<TType?>> expression)
            => new QueryBuilder<TType>().SelectExpression(expression);

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public static ISelectQuery<TType, QueryContext<TType>> SelectExpression<TType>(Expression<Func<QueryContext, TType?>> expression)
            => new QueryBuilder<TType>().SelectExp(expression);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        /// <inheritdoc/>
        public ISelectQuery<TType, TContext> Select()
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType)));
            return this;
        }

        /// <inheritdoc/>
        public ISelectQuery<TResult, TContext> Select<TResult>(Action<ShapeBuilder<TResult>> shape)
        {
            var shapeBuilder = new ShapeBuilder<TResult>();
            shape(shapeBuilder);
            AddNode<SelectNode>(new SelectContext(typeof(TResult))
            {
                Shape = shapeBuilder,
                IsFreeObject = typeof(TResult).IsAnonymousType()
            });
            return EnterNewType<TResult>();
        }

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExpression<TNewType>(Expression<Func<TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }

        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExpression<TNewType>(Expression<Func<TContext, TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }


        /// <summary>
        ///     Adds a <c>SELECT</c> statement, selecting the result of a <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TNewType">The resulting type of the expression.</typeparam>
        /// <typeparam name="TQuery">A query containing a result of <typeparamref name="TNewType"/></typeparam>
        /// <param name="expression">The expression on which to select.</param>
        /// <param name="shape">A optional delegate to build the shape for selecting <typeparamref name="TNewType"/>.</param>
        /// <returns>
        ///     A <see cref="ISelectQuery{TNewType, TContext}"/>.
        /// </returns>
        public ISelectQuery<TNewType, TContext> SelectExp<TQuery, TNewType>(
            Expression<Func<TContext, TQuery?>> expression,
            Action<ShapeBuilder<TNewType>>? shape = null
        ) where TQuery : IQuery<TNewType>
        {
            var shapeBuilder = shape is not null ? new ShapeBuilder<TNewType>() : null;

            if (shape is not null && shapeBuilder is not null)
            {
                shape(shapeBuilder);
            }

            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                Shape = shapeBuilder,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });

            return EnterNewType<TNewType>();
        }


        internal ISelectQuery<TNewType, TContext> SelectExp<TNewType, TInitContext>(Expression<Func<TInitContext, TNewType?>> expression)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TType))
            {
                Expression = expression,
                IncludeShape = false,
                IsFreeObject = typeof(TNewType).IsAnonymousType(),
            });
            return EnterNewType<TNewType>();
        }

        ISelectQuery<TNewType, TContext> IQueryBuilder<TType, TContext>.SelectExpression<TNewType, TQuery>(Expression<Func<TContext, TQuery?>> expression, Action<ShapeBuilder<TNewType>>? shape) where TQuery : default
           => SelectExp(expression, shape);

        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
           => Filter(filter);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Offset(long offset)
            => Offset(offset);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Limit(long limit)
            => Limit(limit);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderBy(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
        => OrderBy(false, propertySelector, nullPlacement);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Offset(Expression<Func<TContext, long>> offset)
            => OffsetExp(offset);
        ISelectQuery<TType, TContext> ISelectQuery<TType, TContext>.Limit(Expression<Func<TContext, long>> limit)
            => LimitExp(limit);
    }
}
