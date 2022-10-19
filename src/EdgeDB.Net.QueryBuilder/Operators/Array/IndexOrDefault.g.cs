using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayIndexOrDefault : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "array_get({0}, {1}, <default := {2?}>)";
    }
}