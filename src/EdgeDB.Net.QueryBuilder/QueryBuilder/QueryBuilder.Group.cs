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
    public partial class QueryBuilder<TType, TContext>
    {
        IGroupQuery<Group<TKey, TType>> IGroupable<TType>.GroupBy<TKey>(Expression<Func<TType, TKey>> propertySelector)
        {
            AddNode<GroupNode>(new GroupContext(typeof(TType))
            {
                PropertyExpression = propertySelector
            });
            return EnterNewType<Group<TKey, TType>>();
        }

        IGroupQuery<Group<TKey, TType>> IGroupable<TType>.Group<TKey>(Expression<Func<TType, GroupBuilder, KeyedGroupBuilder<TKey>>> groupBuilder)
        {
            AddNode<GroupNode>(new GroupContext(typeof(TType))
            {
                BuilderExpression = groupBuilder
            });
            return EnterNewType<Group<TKey, TType>>();
        }
    }
}
