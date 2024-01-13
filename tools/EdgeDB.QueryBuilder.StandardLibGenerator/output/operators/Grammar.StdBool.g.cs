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
        public static void Op_831477780(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static void Op_1772372585(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static void Op_224380509(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static void Op_542282978(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static void Op_1029092862(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static void Op_552746660(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static void Op_1101638904(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static void Op_2014087946(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static void Op_1920939254(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("OR", "  ").Append(bParam);
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static void Op_1030047734(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("AND", "  ").Append(bParam);
        }
        [EdgeQLOp("std::NOT")]
        public static void Op_1040239889(QueryStringWriter writer, QueryStringWriter.Proxy vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [EdgeQLOp("std::IN")]
        public static void Op_774576803(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::NOT IN")]
        public static void Op_996393078(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::EXISTS")]
        public static void Op_76290807(QueryStringWriter writer, QueryStringWriter.Proxy sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [EdgeQLOp("std::LIKE")]
        public static void Op_778761682(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::ILIKE")]
        public static void Op_403316484(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static void Op_1350032251(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static void Op_312637322(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
