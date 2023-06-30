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
        public static string Op_1821251477(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_1560166938(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static string Op_916524160(string? lParam, string? rParam)
        {
            return $"{lParam} * {rParam}";
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static string Op_838532643(string? lParam, string? rParam)
        {
            return $"{lParam} / {rParam}";
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static string Op_1452002730(string? nParam, string? dParam)
        {
            return $"{nParam} % {dParam}";
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static string Op_1490443682(string? nParam, string? pParam)
        {
            return $"{nParam} ^ {pParam}";
        }
    }
}
