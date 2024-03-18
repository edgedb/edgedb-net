#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EdgeQLOp("std::++")]
        public static void Op_2018393229(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static void Op_1540091311(QueryWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
    }
}
