#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_428501794(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("+", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_955623857(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("+").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_86237905(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("-", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_1275946902(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("-").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static void Op_216935838(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("*", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static void Op_2012276076(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("/", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static void Op_1971112825(QueryStringWriter writer, QueryStringWriter.Proxy nParam, QueryStringWriter.Proxy dParam)
        {
            writer.Append(nParam).Wrapped("%", "  ").Append(dParam);
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static void Op_244399847(QueryStringWriter writer, QueryStringWriter.Proxy nParam, QueryStringWriter.Proxy pParam)
        {
            writer.Append(nParam).Wrapped("^", "  ").Append(pParam);
        }
    }
}
