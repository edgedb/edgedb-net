using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetStatementDateTime : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "std::datetime_of_statement()";
    }
}
