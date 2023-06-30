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
        public static string Op_430485548(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_1514506142(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static string Op_888031385(string? lParam, string? rParam)
        {
            return $"{lParam} * {rParam}";
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static string Op_836271396(string? lParam, string? rParam)
        {
            return $"{lParam} / {rParam}";
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static string Op_1452132680(string? nParam, string? dParam)
        {
            return $"{nParam} % {dParam}";
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static string Op_1117134349(string? nParam, string? pParam)
        {
            return $"{nParam} ^ {pParam}";
        }
    }
}
