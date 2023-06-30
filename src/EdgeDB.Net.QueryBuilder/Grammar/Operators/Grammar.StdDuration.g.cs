#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_659357244(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static string Op_1721894087(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
    }
}
