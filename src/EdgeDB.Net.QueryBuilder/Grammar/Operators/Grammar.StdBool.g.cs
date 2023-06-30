#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.GreaterThanOrEqual)]
        [EdgeQLOp("std::>=")]
        public static string Op_982035920(string? lParam, string? rParam)
        {
            return $"{lParam} >= {rParam}";
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static string Op_110311205(string? lParam, string? rParam)
        {
            return $"{lParam} > {rParam}";
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static string Op_1925716827(string? lParam, string? rParam)
        {
            return $"{lParam} <= {rParam}";
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static string Op_473614369(string? lParam, string? rParam)
        {
            return $"{lParam} < {rParam}";
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static string Op_1277366004(string? lParam, string? rParam)
        {
            return $"{lParam} = {rParam}";
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static string Op_612128902(string? lParam, string? rParam)
        {
            return $"{lParam} ?= {rParam}";
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static string Op_1392247208(string? lParam, string? rParam)
        {
            return $"{lParam} != {rParam}";
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static string Op_1095332923(string? lParam, string? rParam)
        {
            return $"{lParam} ?!= {rParam}";
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static string Op_1172159866(string? aParam, string? bParam)
        {
            return $"{aParam} OR {bParam}";
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static string Op_1768737658(string? aParam, string? bParam)
        {
            return $"{aParam} AND {bParam}";
        }
        [EdgeQLOp("std::NOT")]
        public static string Op_608305449(string? vParam)
        {
            return $"NOT {vParam}";
        }
        [EdgeQLOp("std::IN")]
        public static string Op_1707470733(string? eParam, string? sParam)
        {
            return $"{eParam} IN {sParam}";
        }
        [EdgeQLOp("std::NOT IN")]
        public static string Op_1331562901(string? eParam, string? sParam)
        {
            return $"{eParam} NOT IN {sParam}";
        }
        [EdgeQLOp("std::EXISTS")]
        public static string Op_381201122(string? sParam)
        {
            return $"EXISTS {sParam}";
        }
        [EdgeQLOp("std::LIKE")]
        public static string Op_382160421(string? stringParam, string? patternParam)
        {
            return $"{stringParam} LIKE {patternParam}";
        }
        [EdgeQLOp("std::ILIKE")]
        public static string Op_373306546(string? stringParam, string? patternParam)
        {
            return $"{stringParam} ILIKE {patternParam}";
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static string Op_1276445092(string? stringParam, string? patternParam)
        {
            return $"{stringParam} NOT LIKE {patternParam}";
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static string Op_1650541771(string? stringParam, string? patternParam)
        {
            return $"{stringParam} NOT ILIKE {patternParam}";
        }
    }
}
