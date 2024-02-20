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
        public static void Op_1159303302(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("+", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Add, ExpressionType.AddChecked)]
        [EdgeQLOp("std::+")]
        public static void Op_314897733(QueryWriter writer, WriterProxy lParam)
        {
            writer.Append("+").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_89428629(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("-", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Subtract, ExpressionType.Negate)]
        [EdgeQLOp("std::-")]
        public static void Op_823298531(QueryWriter writer, WriterProxy lParam)
        {
            writer.Append("-").Append(lParam);
        }
        [EquivalentExpression(ExpressionType.Multiply)]
        [EdgeQLOp("std::*")]
        public static void Op_1165137993(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("*", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.Modulo)]
        [EdgeQLOp("std::%")]
        public static void Op_1358603801(QueryWriter writer, WriterProxy nParam, WriterProxy dParam)
        {
            writer.Append(nParam).Wrapped("%", "  ").Append(dParam);
        }
    }
}
