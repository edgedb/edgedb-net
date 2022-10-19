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
        public override string? Translate(BinaryExpression expression, ExpressionContext context)
        {
            // translate the left and right side of the binary operation
            var left = TranslateExpression(expression.Left, context);
            var right = TranslateExpression(expression.Right, context);

            // special case for exists keyword
            if ((expression.Right is ConstantExpression rightConst && rightConst.Value is null ||
               expression.Left is ConstantExpression leftConst && leftConst.Value is null) &&
               expression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual)
            {
                return $"{(expression.NodeType is ExpressionType.Equal ? "not exists" : "exists")} {(right == "{}" ? left : right)}";
            }

            // Try to get a IEdgeQLOperator for the given binary operator
            if (!TryGetExpressionOperator(expression.NodeType, out var op))
                throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");

            // build the operator
            return op.Build(left, right);
        }
    }
}
