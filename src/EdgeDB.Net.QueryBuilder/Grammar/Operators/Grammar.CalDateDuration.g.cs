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
        public static string Op_1786602707(string? lParam, string? rParam)
        {
            return $"{lParam} - {rParam}";
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static string Op_369913934(string? lParam, string? rParam)
        {
            return $"{lParam} + {rParam}";
        }
    }
}
