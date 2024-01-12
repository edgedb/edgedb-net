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
        public static void Op_1638544350(QueryStringWriter writer, QueryStringWriter.Proxy sParam)
        {
            writer.Append("DISTINCT").Append(sParam);
        }
        [EdgeQLOp("std::UNION")]
        public static void Op_2060487352(QueryStringWriter writer, QueryStringWriter.Proxy s1Param, QueryStringWriter.Proxy s2Param)
        {
            writer.Append(s1Param).Wrapped("UNION", "  ").Append(s2Param);
        }
        [EdgeQLOp("std::EXCEPT")]
        public static void Op_1858313669(QueryStringWriter writer, QueryStringWriter.Proxy s1Param, QueryStringWriter.Proxy s2Param)
        {
            writer.Append(s1Param).Wrapped("EXCEPT", "  ").Append(s2Param);
        }
        [EdgeQLOp("std::INTERSECT")]
        public static void Op_1588561179(QueryStringWriter writer, QueryStringWriter.Proxy s1Param, QueryStringWriter.Proxy s2Param)
        {
            writer.Append(s1Param).Wrapped("INTERSECT", "  ").Append(s2Param);
        }
        [EquivalentExpression(ExpressionType.Coalesce)]
        [EdgeQLOp("std::??")]
        public static void Op_1663167675(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("??", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Conditional)]
        [EdgeQLOp("std::IF")]
        public static void Op_799215545(QueryStringWriter writer, QueryStringWriter.Proxy if_trueParam, QueryStringWriter.Proxy conditionParam, QueryStringWriter.Proxy if_falseParam)
        {
            writer.Append(if_trueParam).Append(" IF ").Append(conditionParam).Append(" ELSE ").Append(if_falseParam);
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static void Op_1064355627(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
    }
}
