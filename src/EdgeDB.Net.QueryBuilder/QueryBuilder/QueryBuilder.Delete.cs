using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static partial class QueryBuilder
    {
        /// <inheritdoc cref="IQueryBuilder{TType, QueryContext}.Delete"/>
        public static IDeleteQuery<TType, QueryContext<TType>> Delete<TType>()
            => new QueryBuilder<TType>().Delete;
    }

    public partial class QueryBuilder<TType, TContext>
    {
        /// <inheritdoc/>
        [DebuggerHidden]
        public IDeleteQuery<TType, TContext> Delete
        {
            get
            {
                AddNode<DeleteNode>(new DeleteContext(typeof(TType)));
                return this;
            }
        }

        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Filter(Expression<Func<TType, bool>> filter)
            => Filter(filter);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Offset(long offset)
            => Offset(offset);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Limit(long limit)
            => Limit(limit);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Filter(Expression<Func<TType, TContext, bool>> filter)
            => Filter(filter);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderBy(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(true, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.OrderByDesending(Expression<Func<TType, TContext, object?>> propertySelector, OrderByNullPlacement? nullPlacement)
            => OrderBy(false, propertySelector, nullPlacement);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Offset(Expression<Func<TContext, long>> offset)
            => OffsetExp(offset);
        IDeleteQuery<TType, TContext> IDeleteQuery<TType, TContext>.Limit(Expression<Func<TContext, long>> limit)
            => LimitExp(limit);
    }
}
