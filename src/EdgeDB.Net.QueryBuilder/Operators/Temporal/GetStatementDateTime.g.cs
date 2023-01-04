using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetStatementDateTime : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "std::datetime_of_statement()";
    }
}
