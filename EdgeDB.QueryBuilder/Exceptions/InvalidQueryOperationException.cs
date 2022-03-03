using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Thrown when the current method would result in a invalid query being constructed
    /// </summary>
    public class InvalidQueryOperationException : Exception
    {
        public IReadOnlyCollection<QueryExpressionType> ExpressionValidAfter { get; }
        public QueryExpressionType Expression { get; }

        public InvalidQueryOperationException(QueryExpressionType expression, string message)
            : base(message)
        {
            Expression = expression;
            ExpressionValidAfter = new QueryExpressionType[0];
        }

        public InvalidQueryOperationException(QueryExpressionType expression, QueryExpressionType[] validAfter) 
            : base($"Expression {expression} is only valid after {string.Join(", ", validAfter)}")
        {
            Expression = expression;
            ExpressionValidAfter = validAfter;
        }

    }
}
