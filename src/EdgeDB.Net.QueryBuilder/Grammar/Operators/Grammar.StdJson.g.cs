#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static string Op_1422860995(string? lParam, string? rParam)
        {
            return $"{lParam}[{rParam}]";
        }
        [EdgeQLOp("std::++")]
        public static string Op_1012565951(string? lParam, string? rParam)
        {
            return $"{lParam} ++ {rParam}";
        }
    }
}
