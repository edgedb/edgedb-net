using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringReplace : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "re_replace({0}, {1}, {2}, <flags := {3?}>)";
    }
}
