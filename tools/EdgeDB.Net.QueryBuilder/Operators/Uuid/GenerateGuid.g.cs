using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class UuidGenerateGuid : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "uuid_generate_v1mc()";
    }
}
