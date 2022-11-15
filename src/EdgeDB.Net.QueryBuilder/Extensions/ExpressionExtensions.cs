using System;
using System.Linq.Expressions;

namespace EdgeDB
{
    internal static class ExpressionExtensions
    {
        public static TExpression Expect<TExpression>(this Expression expression)
            where TExpression : Expression
        {
            if (expression is not TExpression expected)
                throw new ArgumentException($"Expected {typeof(TExpression)} but got {expression.GetType()}", nameof(expression));

            return expected;
        }
    }
}

