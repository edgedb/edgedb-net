using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class LocalReference : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => ".{0}";

        public string Build(params object[] args)
        {
            throw new NotImplementedException("LocalReference does not have an implementation and should never be called");
        }
    }
}
