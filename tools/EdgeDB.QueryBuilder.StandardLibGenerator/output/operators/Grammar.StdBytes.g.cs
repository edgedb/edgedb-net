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
        public static void Op_595694101(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
        [EquivalentExpression(ExpressionType.ArrayIndex)]
        [EdgeQLOp("std::[]")]
        public static void Op_1337326916(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
    }
}
