using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public abstract class BaseGroupBuilder
    {
        public Expression? UsingExpression { get; protected set; }

        public Expression? ByExpression { get; protected set; }

        internal BaseGroupBuilder() { }
        internal BaseGroupBuilder(Expression @using) { UsingExpression = @using; }
    }
    public class GroupBuilder : BaseGroupBuilder
    {
        public GroupBuilder<T> Using<T>(Expression<Func<T>> @using)
            => new(@using);

        public KeyedGroupBuilder<TKey> By<TKey>(Expression<Func<TKey>> keySelector)
            => new(keySelector);
    }
    public class GroupBuilder<TContext> : BaseGroupBuilder
    {
        public GroupBuilder(Expression @using) : base(@using) { }

        public KeyedContextGroupBuilder<TKey, TContext> By<TKey>(Expression<Func<TContext, TKey>> keySelector)
            => new(keySelector, UsingExpression!);

        public KeyedContextGroupBuilder<TKey, TContext> By<TKey>(Expression<Func<TKey>> keySelector)
            => new(keySelector, UsingExpression!);
    }
    public class KeyedGroupBuilder<TKey> : BaseGroupBuilder
    {
        public KeyedGroupBuilder(Expression keySelector)
            : base()
        {
            ByExpression = keySelector;
        }
    }
    public class KeyedContextGroupBuilder<TKey, TContext> : KeyedGroupBuilder<TKey>
    {
        public KeyedContextGroupBuilder(Expression keySelector, Expression @using)
            : base(keySelector)
        {
            UsingExpression = @using;
        }
    }
}
