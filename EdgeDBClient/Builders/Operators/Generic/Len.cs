using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class Len : IEdgeQLOperator
    {
        public ExpressionType Operator => ExpressionType.ArrayLength;

        public string EdgeQLOperator => "len()";

        public string Build(params object[] args)
        {
            // remove '.Length.
            return $"len({args[0].ToString()!.Replace(".Length", "")})";
        }
    }
}
