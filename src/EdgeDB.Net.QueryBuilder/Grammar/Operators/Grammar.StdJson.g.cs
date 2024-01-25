#nullable restore
using EdgeDB;
using EdgeDB.DataTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal partial class Grammar
    {
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static void Op_524416656(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
        [EdgeQLOp("std::++")]
        public static void Op_1876181017(QueryStringWriter writer, WriterProxy lParam, WriterProxy rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
