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
        public static string Op_471912731(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_946836820(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static string Op_849376209(string? lParam, string? rParam)
        {
            return $"{lParam} * {rParam}";
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static string Op_962461352(string? lParam, string? rParam)
        {
            return $"{lParam} / {rParam}";
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static string Op_590467972(string? nParam, string? dParam)
        {
            return $"{nParam} % {dParam}";
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static string Op_1198838843(string? nParam, string? pParam)
        {
            return $"{nParam} ^ {pParam}";
        }
    }
}
