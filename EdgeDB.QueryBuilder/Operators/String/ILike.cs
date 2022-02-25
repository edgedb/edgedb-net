using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class ILike : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;

        public string EdgeQLOperator => "ilike";

        public string Build(params object[] args)
        {
            return $"{args[0]} ilike {args[1]}";
        }
    }
}
