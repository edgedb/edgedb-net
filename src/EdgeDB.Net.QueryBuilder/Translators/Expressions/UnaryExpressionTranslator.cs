using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a unary operator.
    /// </summary>
    internal class UnaryExpressionTranslator : ExpressionTranslator<UnaryExpression>
    {
        /// <inheritdoc/>
        public override void Translate(UnaryExpression expression, ExpressionContext context, QueryWriter writer)
        {
            switch (expression.NodeType)
            {
                // quote expressions are literal funcs (im pretty sure), so we can just
                // directly translate them and return the result.
                case ExpressionType.Quote when expression.Operand is LambdaExpression lambda:
                    TranslateExpression(lambda.Body, context.Enter(x => x.StringWithoutQuotes = false), writer);
                    return;
                // convert is a type change, so we translate the dotnet form '(type)value' to '<type>value'
                case ExpressionType.Convert:
                {
                    // this is a selector-based expression converting value types to objects, for
                    // this case we can just return the value
                    if (expression.Type == typeof(object))
                        return;

                    // dotnet nullable check
                    if (ReflectionUtils.IsSubclassOfRawGeneric(typeof(Nullable<>), expression.Type) &&
                        expression.Type.GenericTypeArguments[0] == expression.Operand.Type)
                    {
                        // no need to cast in edgedb, return the value
                        return;
                    }

                    var type = EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var edgedbType)
                        ? edgedbType.ToString()
                        : expression.Type.GetEdgeDBTypeName();

                    writer
                        .TypeCast(type)
                        .Append(Proxy(expression.Operand, context));
                    return;
                }
                case ExpressionType.ArrayLength:
                    writer.Append("len(");
                    TranslateExpression(expression.Operand, context, writer);
                    writer.Append(')');
                    return;

                // default case attempts to get an IEdgeQLOperator for the given
                // node type, and uses that to translate the expression.
                default:
                    if (!Grammar.TryBuildOperator(expression.NodeType, writer, Proxy(expression.Operand, context)))
                        throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");
                    return;
            }

            //throw new NotSupportedException($"Failed to find converter for {expression.NodeType}!");
        }
    }
}
