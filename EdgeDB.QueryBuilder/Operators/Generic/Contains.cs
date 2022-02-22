using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class Contains : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;

        public string EdgeQLOperator => throw new NotImplementedException();

        public string Build(params object[] args)
        {
            return $"contains({args[0]}, {args[1]})";
        }
    }
}
