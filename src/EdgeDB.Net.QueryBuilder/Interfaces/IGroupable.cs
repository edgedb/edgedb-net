using EdgeDB.Builders;
using EdgeDB.Interfaces.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    public interface IGroupable<TType>
    {
        IGroupQuery<Group<TKey, TType>> GroupBy<TKey>(Expression<Func<TType, TKey>> propertySelector);

        IGroupQuery<Group<TKey, TType>> Group<TKey>(Expression<Func<TType, GroupBuilder, KeyedGroupBuilder<TKey>>> groupBuilder);
    }
}
