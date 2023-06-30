#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EdgeQLOp("std::++")]
        public static string Op_42256720(string? lParam, string? rParam)
        {
            return $"{lParam} ++ {rParam}";
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static string Op_786656873(string? lParam, string? rParam)
        {
            return $"{lParam}[{rParam}]";
        }
    }
}
