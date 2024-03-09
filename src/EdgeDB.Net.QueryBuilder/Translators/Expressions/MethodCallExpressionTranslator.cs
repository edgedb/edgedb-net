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
        private static string ExpressionAsString(Expression exp)
        {
            var expressionResult = Expression
                .Lambda(exp)
                .Compile()
                .DynamicInvoke();

            if (expressionResult is not string strResult)
                throw new ArgumentException(
                    $"Expected expression {exp} to evaluate to a string, but " +
                    $"got {expressionResult?.GetType().ToString() ?? "NULL"}"
                );

            return strResult;
        }

        public override void Translate(MethodCallExpression expression, ExpressionContext context,
            QueryWriter writer)
        {
            // figure out if the method is something we should translate or something that we should
            // call to pull the result from.
            if (ShouldTranslate(expression, context))
            {
                TranslateToEdgeQL(expression, context, writer);
                return;
            }

            // invoke and translate the result
            var expressionResult = Expression
                .Lambda(expression, context.RootExpression.Parameters)
                .Compile()
                .DynamicInvoke();

            // attempt to get the scalar type of the result of the method.
            if (!EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var type))
            {
                // if we can't, add it as a global
                writer.Marker(MarkerType.Global, context.GetOrAddGlobal(expression, expressionResult));
                return;
                //throw new InvalidOperationException("Expected a scalar type for ");
            }

            // return the variable name containing the result of the method.
            var varName = context.AddVariable(expressionResult);
            writer
                .Marker(MarkerType.Global, varName, Value.Of(writer => writer
                    .TypeCast(type.ToString())
                    .Append(varName)
                ));
        }

        private bool ShouldTranslate(MethodCallExpression expression, ExpressionContext context)
        {
            // if the method references context or a parameter to our current root lambda
            var disassembledInstance = expression.Object is null
                ? Array.Empty<Expression>()
                : ExpressionUtils.DisassembleExpression(expression.Object).ToArray();

            var isInstanceReferenceToContext = expression.Object?.Type == typeof(QueryContext) ||
                                               context.RootExpression.Parameters.Any(x =>
                                                   disassembledInstance.Contains(x));

            var isParameterReferenceToContext = expression.Arguments.Any(x =>
                x.Type == typeof(QueryContext) ||
                context.RootExpression.Parameters.Any(y => ExpressionUtils.DisassembleExpression(x).Contains(y)));

            var isExplicitTranslatorMethod = expression.Method.DeclaringType is not null &&
                                             MethodTranslator.TryGetTranslator(expression.Method.DeclaringType,
                                                 expression.Method.Name, out _);

            var isStdLib = expression.Method.DeclaringType == typeof(EdgeQL);
            return isStdLib || isExplicitTranslatorMethod || isParameterReferenceToContext ||
                   isInstanceReferenceToContext;
        }

        private static void TranslateToEdgeQL(MethodCallExpression expression, ExpressionContext context,
            QueryWriter writer)
        {
            // if our method is within the query context class
            if (expression.Method.DeclaringType?.IsAssignableTo(typeof(IQueryContext)) ?? false)
            {
                switch (expression.Method.Name)
                {
                    case nameof(QueryContext.Global):
                        TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true),
                            writer);
                        return;
                    case nameof(QueryContext.Local):
                    {
                        // get the path as a string and split it into segments
                        var path = ExpressionAsString(expression.Arguments[0]);

                        var pathSegments = path.Split('.');

                        writer.Append('.');

                        for (int i = 0; i != pathSegments.Length; i++)
                        {
                            var prop = (MemberInfo?)context.LocalScope?.GetProperty(pathSegments[i]) ??
                                       context.LocalScope?.GetField(pathSegments[i]) ??
                                       (MemberInfo?)context.NodeContext.CurrentType.GetProperty(pathSegments[i]) ??
                                       context.NodeContext.CurrentType.GetField(pathSegments[i]);

                            if (prop is null)
                                throw new InvalidOperationException(
                                    $"The property \"{pathSegments[i]}\" within \"{path}\" is out of scope"
                                );

                            writer.Append(prop.GetEdgeDBPropertyName());

                            if (i + 1 != pathSegments.Length)
                                writer.Append('.');
                        }

                        return;
                    }
                    case nameof(QueryContext.UnsafeLocal):
                    {
                        // same thing as local except we dont validate anything here
                        writer
                            .Append('.')
                            .Append(ExpressionAsString(expression.Arguments[0]));
                        return;
                    }
                    case nameof(QueryContext.Raw):
                    {
                        // return the raw string as a constant expression and serialize it without quotes
                        writer.Append(ExpressionAsString(expression.Arguments[0]));
                        return;
                    }
                    case nameof(QueryContext.BackLink):
                    {
                        // depending on the backlink method called, we should set some flags:
                        // whether or not the called function is using the string form or the lambda form
                        var isRawPropertyName = expression.Arguments[0].Type == typeof(string);

                        // whether or not a shape argument was supplied
                        var hasShape = !isRawPropertyName && expression.Arguments.Count > 1;

                        writer.Append(".<");

                        // translate the backlink property accessor
                        TranslateExpression(
                            expression.Arguments[0],
                            isRawPropertyName
                                ? context.Enter(x => x.StringWithoutQuotes = true)
                                : context.Enter(x => x.IncludeSelfReference = false),
                            writer
                        );

                        // if its a lambda, add the corresponding generic type as a [is x] statement
                        if (!isRawPropertyName)
                        {
                            writer
                                .Append("[is ")
                                .Append(expression.Method.GetGenericArguments()[0].GetEdgeDBTypeName())
                                .Append(']');
                        }

                        // if it has a shape, translate the shape and add it to the backlink
                        if (hasShape)
                        {
                            writer.Append('{');
                            TranslateExpression(expression.Arguments[1], context, writer);
                            writer.Append('}');
                        }
                    }
                        return;
                    case nameof(QueryContext.SubQuery):
                    {
                        // pull the builder parameter and add it to a new lambda
                        // and execute it to get an instance of the builder
                        var builder =
                            (IQueryBuilder)Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke()!;

                        writer.Wrapped(writer => builder.WriteTo(writer, context));

                        return;
                    }
                    default:
                        throw new NotImplementedException(
                            $"{expression.Method.Name} does not have an implementation. This is a bug, please file a github issue with your query to reproduce this exception.");
                }
            }

            // check if our method translators can translate it
            if (MethodTranslator.TryTranslateMethod(writer, expression, context))
            {
                return;
            }

            throw new Exception($"Couldn't find translator for {expression.Method.Name}");
        }
    }
}
