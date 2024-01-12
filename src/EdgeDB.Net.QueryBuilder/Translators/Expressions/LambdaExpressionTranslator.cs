using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating a lambda expression.
    /// </summary>
    internal class LambdaExpressionTranslator : ExpressionTranslator<LambdaExpression>
    {
        /// <inheritdoc/>
        public override void Translate(LambdaExpression expression, ExpressionContext context, QueryStringWriter result)
        {
            // create a new context and translate the body of the lambda.
            var newContext =
                new ExpressionContext(context.NodeContext, expression, context.QueryArguments, context.Globals);

            TranslateExpression(expression.Body, newContext, result);
        }
    }
}
