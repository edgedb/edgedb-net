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
        public static void Op_1592831365(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("+", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_1961698911(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("+").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_737590747(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("-", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_1427127709(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("-").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static void Op_958112381(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("*", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static void Op_914742331(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("/", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static void Op_1756290400(QueryStringWriter writer, QueryStringWriter.Proxy nParam, QueryStringWriter.Proxy dParam)
        {
            writer.Append(nParam).Wrapped("%", "  ").Append(dParam);
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static void Op_1066702469(QueryStringWriter writer, QueryStringWriter.Proxy nParam, QueryStringWriter.Proxy pParam)
        {
            writer.Append(nParam).Wrapped("^", "  ").Append(pParam);
        }
    }
}
