using EdgeDB.Builders;
using System.Collections;
using System.Linq.Expressions;

namespace EdgeDB
{
    public class EdgeDBQueryable<T> : EdgeDBQueryable, IQueryable<T>
    {
        internal EdgeDBQueryable(EdgeDBQueryProvider provider)
            : base(typeof(T), provider)
        {
            // create default shape
            provider.Shape = BaseShapeBuilder.CreateDefault(typeof(T));
        }

        internal EdgeDBQueryable(EdgeDBQueryProvider provider, Expression expression)
            : base(typeof(T), provider, expression)
        {
        }

        public EdgeDBQueryable<T> Shape(Func<ShapeBuilder<T>> shape)
        {
            return this;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
    }

    public class EdgeDBQueryable : IQueryable
    {
        public Type ElementType { get; }
        public Expression Expression { get; }
        internal EdgeDBQueryProvider Provider { get; }

        internal EdgeDBQueryable(Type type, EdgeDBQueryProvider provider)
        {
            ElementType = type;
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        internal EdgeDBQueryable(Type type, EdgeDBQueryProvider provider, Expression expression)
        {
            ElementType = type;
            Expression = expression;
            Provider = provider;
        }

        public IEnumerator GetEnumerator() => throw new NotImplementedException();

        IQueryProvider IQueryable.Provider => Provider;
    }
}
