using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class VariablesReference : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0}";

        public string Build(params object[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("Cannot use variable with more or less than one argument");

            if (args[0] is not string str)
                throw new InvalidOperationException($"Cannot use {args[0].GetType()} as an argument name");

            return str[1..^1];
        }
    }
}
