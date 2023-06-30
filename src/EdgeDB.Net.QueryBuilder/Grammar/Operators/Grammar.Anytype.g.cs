#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EdgeQLOp("std::DISTINCT")]
        public static string Op_1444996797(string? sParam)
        {
            return $"DISTINCT {sParam}";
        }
        [EdgeQLOp("std::UNION")]
        public static string Op_859135907(string? s1Param, string? s2Param)
        {
            return $"{s1Param} UNION {s2Param}";
        }
        [EdgeQLOp("std::EXCEPT")]
        public static string Op_238456542(string? s1Param, string? s2Param)
        {
            return $"{s1Param} EXCEPT {s2Param}";
        }
        [EdgeQLOp("std::INTERSECT")]
        public static string Op_420294597(string? s1Param, string? s2Param)
        {
            return $"{s1Param} INTERSECT {s2Param}";
        }
        [EquivalentExpression(ExpressionType.Coalesce)]
        [EdgeQLOp("std::??")]
        public static string Op_25474664(string? lParam, string? rParam)
        {
            return $"{lParam} ?? {rParam}";
        }
        [EquivalentExpression(ExpressionType.Conditional)]
        [EdgeQLOp("std::IF")]
        public static string Op_2035982366(string? if_trueParam, string? conditionParam, string? if_falseParam)
        {
            return $"{if_trueParam} IF {conditionParam} ELSE {if_falseParam}";
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static string Op_424228024(string? lParam, string? rParam)
        {
            return $"{lParam}[{rParam}]";
        }
    }
}
