using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a constant value.
    /// </summary>
    internal class ConstantExpressionTranslator : ExpressionTranslator<ConstantExpression>
    {
        /// <inheritdoc/>
        public override void Translate(
            ConstantExpression expression,
            ExpressionContext context,
            QueryStringWriter writer)
        {
            // return the string form if the context requests its raw string
            // form, otherwise parse the constant value.
            if (context.StringWithoutQuotes && expression.Value is string str)
                writer.Append(str);
            else
                QueryUtils.ParseObject(writer, expression.Value);
        }
    }
}
