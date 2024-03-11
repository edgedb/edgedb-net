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

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(Expression{Func{QueryContext, TType}}, bool)"/>
        public static IInsertQuery<TType, QueryContextSelf<TType>> Insert<TType>(Expression<Func<QueryContextSelf<TType>, TType>> value, bool returnInsertedValue)
            => new QueryBuilder<TType>().Insert(value, returnInsertedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(Expression{Func{QueryContext, TType}})"/>
        public static IInsertQuery<TType, QueryContextSelf<TType>> Insert<TType>(Expression<Func<QueryContextSelf<TType>, TType>> value)
            => new QueryBuilder<TType>().Insert(value);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(TType, bool)"/>
        public static IInsertQuery<TType, QueryContextSelf<TType>> Insert<TType>(TType value, bool returnInsertedValue)
            => new QueryBuilder<TType>().Insert(value, returnInsertedValue);

        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Insert(TType)"/>
        public static IInsertQuery<TType, QueryContextSelf<TType>> Insert<TType>(TType value)
            where TType : class
            => new QueryBuilder<TType>().Insert(value, false);

        public static IInsertQuery<object, QueryContextSelf<object>> Insert(Type type, IDictionary<string, object?> values)
            => new QueryBuilder<object>().Insert(type, values);

        public static IInsertQuery<object, QueryContextSelf<object>> Insert(Type type, IDictionary<string, object?> values,  bool returnInsertedValue)
            => new QueryBuilder<object>().Insert(type, values, returnInsertedValue);

        public static IInsertQuery<T, QueryContextSelf<T>> Insert<T>(Type type, Expression<Func<T>> shape)
            => new QueryBuilder<T>().Insert(type, shape);

        public static IInsertQuery<T, QueryContextSelf<T>> Insert<T>(Type type, Expression<Func<QueryContext, T>> shape)
            => new QueryBuilder<T>().Insert(type, shape);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        public IInsertQuery<TType, TContext> Insert(Type type, LambdaExpression expression,
            bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(type, expression));

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(type), true, insertNode);
            }

            return this;
        }

        public IInsertQuery<TType, TContext> Insert(Type type, IDictionary<string, object?> values, bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(type, InsertNode.InsertValue.FromRaw(type, values)));

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(type), true, insertNode);
            }

            return this;
        }

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(TType value, bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(typeof(TType), value));

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, insertNode);
            }

            return this;
        }

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(TType value)
            => Insert(value, false);

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value, bool returnInsertedValue = true)
        {
            var insertNode = AddNode<InsertNode>(new InsertContext(typeof(TType), value));

            if (returnInsertedValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TType)), true, insertNode);
            }

            return this;
        }

        /// <inheritdoc/>
        public IInsertQuery<TType, TContext> Insert(Expression<Func<TContext, TType>> value)
            => Insert(value, false);

        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflict()
            => UnlessConflict();
        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflictOn(Expression<Func<TType, object?>> propertySelector)
            => UnlessConflictOn(propertySelector);
        IUnlessConflictOn<TType, TContext> IInsertQuery<TType, TContext>.UnlessConflictOn(Expression<Func<TType, TContext, object?>> propertySelector)
            => UnlessConflictOn(propertySelector);
    }
}
