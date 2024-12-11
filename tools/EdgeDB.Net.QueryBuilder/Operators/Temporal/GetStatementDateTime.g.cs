using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetStatementDateTime : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "std::datetime_of_statement()";
    }
}
