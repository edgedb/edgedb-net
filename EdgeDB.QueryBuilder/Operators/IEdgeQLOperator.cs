using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Operators
{
    public interface IEdgeQLOperator
    {
        ExpressionType? Operator { get; }
        string EdgeQLOperator { get; }

        string Build(params object[] args)
        {
            return Regex.Replace(EdgeQLOperator, @"({\d+?})", (m) =>
            {
                return $"{args[int.Parse(m.Groups[1].Value)]}";
            });
        }
    }
}
