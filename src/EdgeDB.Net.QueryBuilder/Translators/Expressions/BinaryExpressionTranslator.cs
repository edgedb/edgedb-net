using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a binary operator.
    /// </summary>
    internal class BinaryExpressionTranslator : ExpressionTranslator<BinaryExpression>
    {
        /// <inheritdoc/>
        public override void Translate(BinaryExpression expression, ExpressionContext context, QueryStringWriter writer)
        {
            // special case for exists keyword
            if ((expression.Right is ConstantExpression { Value: null } ||
               expression.Left is ConstantExpression { Value: null }) &&
               expression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual)
            {
                writer.Append(expression.NodeType is ExpressionType.Equal ? "not exists" : "exists");

                TranslateExpression(
                    expression.Right is ConstantExpression { Value: null }
                        ? expression.Left
                        : expression.Right,
                    context,
                    writer);
            }

            // try to build an operator for the given binary operator
            if (!Grammar.TryBuildOperator(
                    expression.NodeType, writer,
                    Proxy(context, expression.Left, expression.Right))
            ) throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");
        }
    }
}
