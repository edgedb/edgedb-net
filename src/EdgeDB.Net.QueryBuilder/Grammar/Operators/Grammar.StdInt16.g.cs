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
        public static void Op_2042766080(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("+", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_1161724507(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("+").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_1689996485(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("-", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_121616012(QueryStringWriter writer, QueryStringWriter.Proxy lParam)
        {
            writer.Append("-").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static void Op_1767042567(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("*", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static void Op_1327518205(QueryStringWriter writer, QueryStringWriter.Proxy nParam, QueryStringWriter.Proxy dParam)
        {
            writer.Append(nParam).Wrapped("%", "  ").Append(dParam);
        }
    }
}
