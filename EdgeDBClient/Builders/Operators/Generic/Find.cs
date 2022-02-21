using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class Find : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;

        public string EdgeQLOperator => "find()";
            
        public string Build(params object[] args)
        {
            return $"find({args[0]}, {args[1]}{(args.Length > 2 ? $", {args[2]}" : "")})";
        }
    }
}
