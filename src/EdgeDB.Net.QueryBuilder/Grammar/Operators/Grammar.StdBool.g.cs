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
        public static void Op_369647602(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.GreaterThan)]
        [EdgeQLOp("std::>")]
        public static void Op_2443376(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(">", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThanOrEqual)]
        [EdgeQLOp("std::<=")]
        public static void Op_1241587673(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.LessThan)]
        [EdgeQLOp("std::<")]
        public static void Op_1896879884(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("<", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::=")]
        public static void Op_1449313963(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Equal)]
        [EdgeQLOp("std::?=")]
        public static void Op_150009094(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::!=")]
        public static void Op_742503818(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.NotEqual)]
        [EdgeQLOp("std::?!=")]
        public static void Op_1060384284(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("?!=", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.OrElse)]
        [EdgeQLOp("std::OR")]
        public static void Op_867176169(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("OR", "  ").Append(bParam);
        }
        [EquivalentExpression(ExpressionType.AndAlso)]
        [EdgeQLOp("std::AND")]
        public static void Op_422722444(QueryStringWriter writer, QueryStringWriter.Proxy aParam, QueryStringWriter.Proxy bParam)
        {
            writer.Append(aParam).Wrapped("AND", "  ").Append(bParam);
        }
        [EdgeQLOp("std::NOT")]
        public static void Op_972574003(QueryStringWriter writer, QueryStringWriter.Proxy vParam)
        {
            writer.Append("NOT").Append(vParam);
        }
        [EdgeQLOp("std::IN")]
        public static void Op_291531899(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::NOT IN")]
        public static void Op_1830644540(QueryStringWriter writer, QueryStringWriter.Proxy eParam, QueryStringWriter.Proxy sParam)
        {
            writer.Append(eParam).Wrapped("NOT IN", "  ").Append(sParam);
        }
        [EdgeQLOp("std::EXISTS")]
        public static void Op_890479237(QueryStringWriter writer, QueryStringWriter.Proxy sParam)
        {
            writer.Append("EXISTS").Append(sParam);
        }
        [EdgeQLOp("std::LIKE")]
        public static void Op_1543435975(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::ILIKE")]
        public static void Op_1338558277(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("ILIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT LIKE")]
        public static void Op_346941148(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT LIKE", "  ").Append(patternParam);
        }
        [EdgeQLOp("std::NOT ILIKE")]
        public static void Op_1153360559(QueryStringWriter writer, QueryStringWriter.Proxy stringParam, QueryStringWriter.Proxy patternParam)
        {
            writer.Append(stringParam).Wrapped("NOT ILIKE", "  ").Append(patternParam);
        }
    }
}
