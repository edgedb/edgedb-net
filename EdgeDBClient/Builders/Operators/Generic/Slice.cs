using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class Slice : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;

        public string EdgeQLOperator => "[]";

        public string Build(params object[] args)
        {
            long? endIndex = null;

            if (args.Length > 2)
                endIndex = long.Parse(args[2].ToString()!);

            return $"{args[0]}[{args[1]}:{endIndex?.ToString() ?? ""}]";
        }
    }
}
