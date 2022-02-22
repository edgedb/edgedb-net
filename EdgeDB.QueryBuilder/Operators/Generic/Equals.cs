using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    internal class Equals : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Equal;

        public string EdgeQLOperator => "?="; // TODO: maybe change this to be nullable aware?

        public string Build(params object[] args)
        {
            return $"{args[0]} ?= {args[1]}";
        }
    }
}
