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
            QueryStringWriter writer)
        {
            TranslateExpression(expression.IfTrue, context, writer);
            writer.Append(" IF ");
            TranslateExpression(expression.Test, context, writer);
            writer.Append(" ELSE ");
            TranslateExpression(expression.IfFalse, context, writer);
        }
    }
}
