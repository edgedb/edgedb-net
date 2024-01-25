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
        public static void Op_189735960(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("+", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_1302849592(QueryStringWriter writer, WriterProxy lParam)
        {
            writer.Append("+").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_1949138534(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("-", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_905753006(QueryStringWriter writer, WriterProxy lParam)
        {
            writer.Append("-").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static void Op_526517953(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("*", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Divide)]
        [EdgeQLOp("std::/")]
        public static void Op_1611670028(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("/", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static void Op_252483166(QueryStringWriter writer, WriterProxy nParam, WriterProxy dParam)
        {
            writer.Append(nParam).Wrapped("%", "  ").Append(dParam);
        }
        [EquivalentExpression(ExpressionType.Power)]
        [EdgeQLOp("std::^")]
        public static void Op_1530662240(QueryStringWriter writer, WriterProxy nParam, WriterProxy pParam)
        {
            writer.Append(nParam).Wrapped("^", "  ").Append(pParam);
        }
    }
}
