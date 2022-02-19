using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class NotEquals : IEdgeQLOperator
    {
        public ExpressionType Operator => ExpressionType.NotEqual;

        public string EdgeQLOperator => "?!="; // // TODO: maybe change this?

        public string Build(params object[] args)
        {
            return $"{args[0]} ?!= {args[1]}";
        }
    }
}
