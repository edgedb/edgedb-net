using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression creating a new array and
    ///     possibly initializing the elements of the new array.
    /// </summary>
    internal class NewArrayExpressionTranslator : ExpressionTranslator<NewArrayExpression>
    {
        /// <inheritdoc/>
        public override void Translate(NewArrayExpression expression, ExpressionContext context, QueryStringWriter writer)
        {
            var brackets = EdgeDBTypeUtils.IsLink(expression.Type, out _, out _)
                ? "{}"
                : "[]";

            writer.Append(brackets[0]);

            for (var i = 0; i != expression.Expressions.Count;)
            {
                TranslateExpression(expression.Expressions[i], context, writer);

                if (i++ < expression.Expressions.Count)
                    writer.Append(',');
            }

            writer.Append(brackets[1]);
        }
    }
}
