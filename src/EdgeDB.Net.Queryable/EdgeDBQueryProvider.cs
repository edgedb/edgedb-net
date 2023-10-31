using EdgeDB.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class EdgeDBQueryProvider : IQueryProvider
    {
        public IShapeBuilder? Shape { get; set; }

        private readonly List<EdgeDBQueryable> _parts;

        public EdgeDBQueryProvider()
        {
            _parts = new List<EdgeDBQueryable>();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var part = new EdgeDBQueryable(expression.Type, this, expression);
            _parts.Add(part);

            return part;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var part = new EdgeDBQueryable<TElement>(this, expression);
            _parts.Add(part);
            return part;
        }
        public object? Execute(Expression expression)
        {
            return null;
        }
        public TResult Execute<TResult>(Expression expression)
        {
            return default!;
        }

        public TransientQuery ToTransient()
        {
            return new(_parts);
        }
    }
}
