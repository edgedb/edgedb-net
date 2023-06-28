using EdgeDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public partial class QueryBuilder<TType, TContext>
    {
        ISingleCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.ElseReturn()
            => ElseReturnDefault();
        IQueryBuilder<object?, TContext> IUnlessConflictOn<TType, TContext>.Else<TQueryBuilder>(TQueryBuilder elseQuery)
            => ElseJoint(elseQuery);
        IMultiCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.Else(Func<IQueryBuilder<TType, TContext>, IMultiCardinalityExecutable<TType>> elseQuery)
            => Else(elseQuery);
        ISingleCardinalityExecutable<TType> IUnlessConflictOn<TType, TContext>.Else(Func<IQueryBuilder<TType, TContext>, ISingleCardinalityExecutable<TType>> elseQuery)
            => Else(elseQuery);
    }
}
