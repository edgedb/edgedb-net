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
        public static void Op_1633607166(QueryStringWriter writer, WriterProxy sParam)
        {
            writer.Append("DISTINCT").Append(sParam);
        }
        [EdgeQLOp("std::UNION")]
        public static void Op_999725343(QueryStringWriter writer, WriterProxy s1Param, WriterProxy s2Param)
        {
            writer.Append(s1Param).Wrapped("UNION", "  ").Append(s2Param);
        }
        [EdgeQLOp("std::EXCEPT")]
        public static void Op_474586655(QueryStringWriter writer, WriterProxy s1Param, WriterProxy s2Param)
        {
            writer.Append(s1Param).Wrapped("EXCEPT", "  ").Append(s2Param);
        }
        [EdgeQLOp("std::INTERSECT")]
        public static void Op_1866682449(QueryStringWriter writer, WriterProxy s1Param, WriterProxy s2Param)
        {
            writer.Append(s1Param).Wrapped("INTERSECT", "  ").Append(s2Param);
        }
        [EquivalentExpression(ExpressionType.Coalesce)]
        [EdgeQLOp("std::??")]
        public static void Op_240810131(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("??", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Conditional)]
        [EdgeQLOp("std::IF")]
        public static void Op_772749472(QueryStringWriter writer, WriterProxy if_trueParam, WriterProxy conditionParam, WriterProxy if_falseParam)
        {
            writer.Append(if_trueParam).Append(" IF ").Append(conditionParam).Append(" ELSE ").Append(if_falseParam);
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static void Op_861836053(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
    }
}
