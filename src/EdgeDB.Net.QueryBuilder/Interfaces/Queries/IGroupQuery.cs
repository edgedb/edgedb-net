using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface IGroupQuery<TType, TContext> where TContext : IQueryContext
    {
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TKey>> selector);

        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TType, TContext, TKey>> selector);

        IGroupUsingQuery<TType, GroupContext<TUsing, TContext>> Using<TUsing>(
            Expression<Func<TType, TUsing>> expression);

        IGroupUsingQuery<TType, GroupContext<TUsing, TContext>> Using<TUsing>(
            Expression<Func<TType, TContext, TUsing>> expression);
    }

    public interface IGroupUsingQuery<TType, TContext>
    {
        IMultiCardinalityExecutable<Group<TKey, TType>> By<TKey>(Expression<Func<TContext, TKey>> selector);
    }
}
