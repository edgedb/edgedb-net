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
        public static void Op_817783219(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped(rParam, "[]");
        }
        [EdgeQLOp("std::++")]
        public static void Op_117666539(QueryStringWriter writer, QueryStringWriter.Proxy lParam, QueryStringWriter.Proxy rParam)
        {
            writer.Append(lParam).Wrapped("++", "  ").Append(rParam);
        }
    }
}
