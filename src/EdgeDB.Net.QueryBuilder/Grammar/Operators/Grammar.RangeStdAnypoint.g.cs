#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static string Op_1561525116(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_544304484(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static string Op_717103762(string? lParam, string? rParam)
        {
            return $"{lParam} * {rParam}";
        }
    }
}
