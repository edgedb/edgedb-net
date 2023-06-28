using EdgeDB.Interfaces;
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
        /// <inheritdoc cref="IQueryBuilder{TType, TContext}.For(IEnumerable{TType}, Expression{Func{JsonCollectionVariable{TType}, IQueryBuilder}})"/>
        public static IMultiCardinalityExecutable<TType> For<TType>(IEnumerable<TType> collection,
            Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator)
            => new QueryBuilder<TType>().For(collection, iterator);
    }

    public partial class QueryBuilder<TType, TContext>
    {
        public IMultiCardinalityExecutable<TType> For(IEnumerable<TType> collection, Expression<Func<JsonCollectionVariable<TType>, IQueryBuilder>> iterator)
        {
            AddNode<ForNode>(new ForContext(typeof(TType))
            {
                Expression = iterator,
                Set = collection
            });

            return this;
        }
    }
}
