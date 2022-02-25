using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class PadLeft : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;

        public string EdgeQLOperator => "str_pad_start()";

        public string Build(params object[] args)
        {
            return $"str_pad_start({args[0]})";
        }
    }
}
