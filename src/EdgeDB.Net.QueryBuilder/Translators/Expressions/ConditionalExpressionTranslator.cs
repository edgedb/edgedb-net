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
            QueryWriter writer)
        {
            writer.Append(
                Proxy(expression.IfTrue, context),
                " IF ",
                Proxy(expression.Test, context),
                " ELSE ",
                Proxy(expression.IfFalse, context)
            );
        }
    }
}
