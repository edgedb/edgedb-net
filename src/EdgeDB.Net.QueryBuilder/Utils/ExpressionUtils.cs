using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB;

/// <summary>
///     A class of utility functions for working with expressions.
/// </summary>
internal static class ExpressionUtils
{
    internal static MemberInfo GetMemberSelection(LambdaExpression expression)
    {
        if (expression.Body is not MemberExpression member)
            throw new InvalidOperationException("The body of the expression must be a member expression");

        return member.Member;
    }

    /// <summary>
    ///     Disassembles an arbitrary expression into a list of expression nodes.
    /// </summary>
    /// <param name="expression">The expression to disassemble.</param>
    /// <returns>
    ///     A collection of expressions representing the passed in expression.
    /// </returns>
    public static IEnumerable<Expression> DisassembleExpression(Expression expression)
    {
        //  return the "root" expression
        yield return expression;

        // while the current expression is a member expression, grab its child expression and yield it.
        var temp = expression;
        while (temp is MemberExpression memberExpression)
        {
            if (memberExpression.Expression is not null)
            {
                yield return memberExpression.Expression;
                temp = memberExpression.Expression;
            }
            else
                break;
        }
    }
}
