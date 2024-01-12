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
        public static void Op_729200035(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static void Op_1046046268(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static void Op_1597853055(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static void Op_578445976(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static void Op_211140160(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static void Op_400835212(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static void Op_1477175795(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static void Op_1573161849(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static void Op_583167865(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("OR", "  ").Append(bParam);
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static void Op_12456449(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("AND", "  ").Append(bParam);
        }
        [EdgeQLOp("std::NOT")]
        public static void Op_1285706761(QueryStringWriter writer, QueryStringWriter.Proxy vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [EdgeQLOp("std::IN")]
        public static void Op_2014300023(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::NOT IN")]
        public static void Op_1999305461(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::EXISTS")]
        public static void Op_1994021681(QueryStringWriter writer, QueryStringWriter.Proxy sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [EdgeQLOp("std::LIKE")]
        public static void Op_210486214(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::ILIKE")]
        public static void Op_1921382656(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static void Op_1511415837(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static void Op_553368690(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
