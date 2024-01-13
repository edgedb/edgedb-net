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
        public static void Op_1937429070(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static void Op_1624245694(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static void Op_821184664(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static void Op_815185890(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static void Op_1697748090(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static void Op_1561619423(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static void Op_838504627(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static void Op_1914177547(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static void Op_13096253(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("OR", "  ").Append(bParam);
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static void Op_1518803777(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("AND", "  ").Append(bParam);
        }
        [EdgeQLOp("std::NOT")]
        public static void Op_2098528833(QueryStringWriter writer, QueryStringWriter.Proxy vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [EdgeQLOp("std::IN")]
        public static void Op_868007923(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::NOT IN")]
        public static void Op_1504312452(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::EXISTS")]
        public static void Op_1204328176(QueryStringWriter writer, QueryStringWriter.Proxy sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [EdgeQLOp("std::LIKE")]
        public static void Op_763317737(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::ILIKE")]
        public static void Op_1462943650(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static void Op_622164578(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static void Op_1584628446(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
