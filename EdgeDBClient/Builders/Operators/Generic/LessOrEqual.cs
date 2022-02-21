using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class LessOrEqual : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.LessThanOrEqual;

        public string EdgeQLOperator => "<=";

        public string Build(params object[] args)
        {
            return $"{args[0]} <= {args[1]}";
        }
    }
}
