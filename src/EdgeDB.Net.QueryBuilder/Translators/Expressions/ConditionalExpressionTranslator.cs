using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a conditional operator.
    /// </summary>
    internal class ConditionalExpressionTranslator : ExpressionTranslator<ConditionalExpression>
    {
        /// <inheritdoc/>
        public override void Translate(
            ConditionalExpression expression,
            ExpressionContext context,
            StringBuilder result)
        {
            TranslateExpression(expression.IfTrue, context, result);
            result.Append(" IF ");
            TranslateExpression(expression.Test, context, result);
            result.Append(" ELSE ");
            TranslateExpression(expression.IfFalse, context, result);
        }
    }
}
