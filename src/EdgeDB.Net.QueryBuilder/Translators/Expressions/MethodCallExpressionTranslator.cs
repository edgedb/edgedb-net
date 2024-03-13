using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a method
    ///     call to either static or an instance method.
    /// </summary>
    internal class MethodCallExpressionTranslator : ExpressionTranslator<MethodCallExpression>
    {
        private bool IsIllegalToInvoke(MethodCallExpression expression, ExpressionContext context)
        {
            // if any arguments reference context or json variables or root parameters
            if (expression.Arguments.Any(x =>
                {
                    var disassembled = ExpressionUtils.DisassembleExpression(x);

                    return disassembled.Any(y =>
                        y.Type.IsAssignableTo(typeof(IQueryContext)) || y.Type.IsAssignableTo(typeof(IJsonVariable)) ||
                        context.RootExpression.Parameters.Contains(y));
                }))
                return true;

            if(expression.Object is not null)
            {
                var disassembledObject = ExpressionUtils.DisassembleExpression(expression.Object).ToArray();

                // if instance references context or json variables
                if (disassembledObject.Any(x =>
                        x.Type.IsAssignableTo(typeof(IQueryContext)) || x.Type.IsAssignableTo(typeof(IJsonVariable))))
                    return true;

                // if instance references any root expression parameters.
                if (context.RootExpression.Parameters.Any(x => disassembledObject.Contains(x)))
                    return true;
            }

            return false;
        }

        public override void Translate(MethodCallExpression expression, ExpressionContext context,
            QueryWriter writer)
        {
            // figure out if the method is something we should translate or something that we should
            // call to pull the result from.
            if (MethodTranslator.TryGetTranslator(expression, out var translator))
            {
                translator.Translate(writer, expression, context);
                return;
            }

            if(IsIllegalToInvoke(expression, context))
                throw new InvalidOperationException($"Cannot invoke {expression.Method.Name} because it references mock-instances; and it has no translator");

            // invoke and translate the result
            var expressionResult = Expression
                .Lambda(expression, context.RootExpression.Parameters)
                .Compile()
                .DynamicInvoke(new object[context.RootExpression.Parameters.Count]);

            // attempt to get the scalar type of the result of the method.
            if (!EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var type))
            {
                // if we can't, add it as a global
                writer.Marker(MarkerType.GlobalReference, context.GetOrAddGlobal(expression, expressionResult));
                return;
                //throw new InvalidOperationException("Expected a scalar type for ");
            }

            // return the variable name containing the result of the method.
            var varName = context.AddVariable(expressionResult);
            writer.Marker(MarkerType.GlobalReference, varName, Value.Of(writer => writer
                .TypeCast(type.ToString())
                .Append(varName)
            ));
        }
    }
}
