using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class GreaterThan : IEdgeQLOperator
    {
        public ExpressionType Operator => ExpressionType.GreaterThan;

        public string EdgeQLOperator => throw new NotImplementedException();

        public string Build(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
