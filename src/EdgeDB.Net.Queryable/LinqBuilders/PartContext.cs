using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.LinqBuilders
{
    internal sealed class PartContext
    {
        public EdgeDBQueryable Queryable { get; }
        public Expression[] Parameters { get; }
        public Expression? Ancestor { get; }

        public MethodCallExpression MethodCallExpression
            => Queryable.Expression is MethodCallExpression mce
                ? mce
                : throw new NotSupportedException();

        public PartContext(EdgeDBQueryable queryable)
        {
            Queryable = queryable;

            if(queryable.Expression is MethodCallExpression mce)
            {
                Parameters = mce.Arguments.Skip(1).ToArray();
                Ancestor = mce.Arguments[0];
            }

            Parameters ??= Array.Empty<Expression>();
        }
    }
}
