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
        public static string Op_1528073750(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static string Op_1851082985(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static string Op_83229196(string? lParam, string? rParam)
        {
            return $"{lParam} * {rParam}";
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static string Op_386476964(string? nParam, string? dParam)
        {
            return $"{nParam} % {dParam}";
        }
    }
}
