using EdgeDB.Interfaces.Queries;
using EdgeDB.LinqBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class TransientQuery
    {
        private static readonly ConcurrentDictionary<Type, Type> _genericBuildersCache = new();

        private readonly List<EdgeDBQueryable> _parts;

        public TransientQuery(List<EdgeDBQueryable> parts)
        {
            _parts = parts;
        }

        public IQueryBuilder Compile()
        {
            // the first part is always the part containing the type to select
            var initialType = _parts[0].ElementType;

            var queryBuilder = new GenericlessQueryBuilder(initialType)
                .Select(_parts[0].Provider.Shape);

            using(var linqBuilder = LinqBuilder.CreateBuilder())
            {
                foreach(var part in _parts.Skip(1))
                {
                    linqBuilder.Process(part);
                }

                queryBuilder = linqBuilder.Compile(queryBuilder);
            }

            return queryBuilder;
        }

    }
}
