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
        public static void Op_1845142749(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped(">=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static void Op_81595436(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped(">", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static void Op_479804388(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("<=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static void Op_1489707051(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("<", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static void Op_1935164831(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static void Op_1850234448(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("?=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static void Op_501502648(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static void Op_139905229(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("?!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static void Op_1649942865(QueryWriter writer, WriterProxy aParam, WriterProxy bParam)
        {
            writer.Append(aParam).Wrapped("OR", "  ").Append(bParam);
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static void Op_1863219856(QueryWriter writer, WriterProxy aParam, WriterProxy bParam)
        {
            writer.Append(aParam).Wrapped("AND", "  ").Append(bParam);
        }
        [EdgeQLOp("std::NOT")]
        public static void Op_948960421(QueryWriter writer, WriterProxy vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [EdgeQLOp("std::IN")]
        public static void Op_1745059489(QueryWriter writer, WriterProxy eParam, WriterProxy sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::NOT IN")]
        public static void Op_1614153116(QueryWriter writer, WriterProxy eParam, WriterProxy sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::EXISTS")]
        public static void Op_1462523289(QueryWriter writer, WriterProxy sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [EdgeQLOp("std::LIKE")]
        public static void Op_1910355345(QueryWriter writer, WriterProxy stringParam, WriterProxy patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::ILIKE")]
        public static void Op_447899405(QueryWriter writer, WriterProxy stringParam, WriterProxy patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static void Op_566786037(QueryWriter writer, WriterProxy stringParam, WriterProxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static void Op_1342537544(QueryWriter writer, WriterProxy stringParam, WriterProxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
