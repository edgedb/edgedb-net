using EdgeDB.Builders;
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
        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>()
            => new QueryBuilder<T>().GroupInternal<T>();

        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>(Action<ShapeBuilder<T>> shape)
            => new QueryBuilder<T>().GroupInternal(shape: shape);

        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>(Expression<Func<T>> selector)
            => new QueryBuilder<T>().GroupInternal<T>(selector);

        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>(Expression<Func<QueryContext, T>> selector)
            => new QueryBuilder<T>().GroupInternal<T>(selector);

        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>(Expression<Func<T>> selector, Action<ShapeBuilder<T>> shape)
            => new QueryBuilder<T>().GroupInternal(selector, shape);

        public static IGroupQuery<T, QueryContextSelf<T>> Group<T>(Expression<Func<QueryContext, T>> selector, Action<ShapeBuilder<T>> shape)
            => new QueryBuilder<T>().GroupInternal(selector, shape);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        internal IGroupQuery<TResult, TContext> GroupInternal<TResult>(LambdaExpression? selector = null,
            Action<ShapeBuilder<TResult>>? shape = null)
        {
            ShapeBuilder<TResult>? shapeBuilder = shape is not null ? new() : null;
            shape?.Invoke(shapeBuilder!);

            AddNode<GroupNode>(new GroupContext(typeof(TType))
            {
                Selector = selector,
                Shape = shapeBuilder
            });

            return EnterNewType<TResult>();
        }

        public IGroupQuery<TType, TContext> Group()
            => GroupInternal<TType>();

        public IGroupQuery<TResult, TContext> Group<TResult>(Action<ShapeBuilder<TResult>> shape)
            => GroupInternal(shape: shape);

        IGroupQuery<TResult, TContext> IQueryBuilder<TType, TContext>.Group<TResult>(Expression<Func<TResult>> selector)
            => GroupInternal<TResult>(selector);

        IGroupQuery<TResult, TContext> IQueryBuilder<TType, TContext>.Group<TResult>(Expression<Func<TContext, TResult>> selector)
            => GroupInternal<TResult>(selector);

        IGroupQuery<TResult, TContext> IQueryBuilder<TType, TContext>.Group<TResult>(Expression<Func<TResult>> selector, Action<ShapeBuilder<TResult>> shape)
            => GroupInternal(selector, shape);

        IGroupQuery<TResult, TContext> IQueryBuilder<TType, TContext>.Group<TResult>(Expression<Func<TContext, TResult>> selector, Action<ShapeBuilder<TResult>> shape)
            => GroupInternal(selector, shape);

        IMultiCardinalityExecutable<Group<TKey, TType>> IGroupQuery<TType, TContext>.By<TKey>(Expression<Func<TType, TKey>> selector)
            => By(selector).EnterNewType<Group<TKey, TType>>();

        IMultiCardinalityExecutable<Group<TKey, TType>> IGroupQuery<TType, TContext>.By<TKey>(Expression<Func<TType, TContext, TKey>> selector)
            => By(selector).EnterNewType<Group<TKey, TType>>();


        IGroupUsingQuery<TType, GroupContext<TUsing, TContext>> IGroupQuery<TType, TContext>.Using<TUsing>(
            Expression<Func<TType, TUsing>> expression)
            => Using<GroupContext<TUsing, TContext>, TUsing>(expression);

        IGroupUsingQuery<TType, GroupContext<TUsing, TContext>> IGroupQuery<TType, TContext>.Using<TUsing>(Expression<Func<TType, TContext, TUsing>> expression) => throw new NotImplementedException();

        IMultiCardinalityExecutable<Group<TKey, TType>> IGroupUsingQuery<TType, TContext>.By<TKey>(
            Expression<Func<TContext, TKey>> selector)
            => By(selector).EnterNewType<Group<TKey, TType>>();
    }
}
