using EdgeDB.Operators;
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

        public override void Translate(MethodCallExpression expression, ExpressionContext context, StringBuilder result)
        {
            // figure out if the method is something we should translate or something that we should
            // call to pull the result from.
            if (ShouldTranslate(expression, context))
            {
                TranslateToEdgeQL(expression, context, result);
                return;
            }

            // invoke and translate the result
            var expressionResult = Expression
                .Lambda(expression, context.RootExpression.Parameters)
                .Compile()
                .DynamicInvoke();

            // attempt to get the scalar type of the result of the method.
            if (!EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var type))
                throw new InvalidOperationException($"Cannot use {expression.Type} as a result in an un-translated context");

            // return the variable name containing the result of the method.
            result.TypeCast(type)
                .Append(context.AddVariable(expressionResult));
        }

        private bool ShouldTranslate(MethodCallExpression expression, ExpressionContext context)
        {
            // if the method references context or a parameter to our current root lambda
            var disassembledInstance = expression.Object is null
                ? Array.Empty<Expression>()
                : ExpressionUtils.DisassembleExpression(expression.Object).ToArray();

            var isInstanceReferenceToContext = expression.Object?.Type == typeof(QueryContext) || context.RootExpression.Parameters.Any(x => disassembledInstance.Contains(x));
            var isParameterReferenceToContext = expression.Arguments.Any(x => x.Type == typeof(QueryContext) || context.RootExpression.Parameters.Any(y => ExpressionUtils.DisassembleExpression(x).Contains(y)));
            var isExplicitTranslatorMethod = expression.Method.GetCustomAttribute<EquivalentOperator>() is not null;
            var isStdLib = expression.Method.DeclaringType == typeof(EdgeQL);
            return isStdLib || isExplicitTranslatorMethod || isParameterReferenceToContext || isInstanceReferenceToContext;
        }

        private static void TranslateToEdgeQL(MethodCallExpression expression, ExpressionContext context, StringBuilder result)
        {
            // if our method is within the query context class
            if (expression.Method.DeclaringType?.IsAssignableTo(typeof(IQueryContext)) ?? false)
            {
                switch (expression.Method.Name)
                {
                    case nameof(QueryContext.Global):
                        TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true), result);
                        return;
                    case nameof(QueryContext.Local):
                        {
                            // get the path as a string and split it into segments
                            var path = ExpressionAsString(expression.Arguments[0]);

                            var pathSegments = path.Split('.');

                            result.Append('.');

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

                                result.Append(prop.GetEdgeDBPropertyName());
                                if (i + 1 != pathSegments.Length)
                                    result.Append('.');
                            }

                            return;
                        }
                    case nameof(QueryContext.UnsafeLocal):
                        {
                            // same thing as local except we dont validate anything here
                            result.Append('.');
                            result.Append(ExpressionAsString(expression.Arguments[0]));
                            return;
                        }
                    case nameof(QueryContext.Raw):
                        {
                            // return the raw string as a constant expression and serialize it without quotes
                            result.Append(ExpressionAsString(expression.Arguments[0]));
                            return;
                        }
                    case nameof(QueryContext.BackLink):
                        {
                            // depending on the backlink method called, we should set some flags:
                            // whether or not the called function is using the string form or the lambda form
                            var isRawPropertyName = expression.Arguments[0].Type == typeof(string);

                            // whether or not a shape argument was supplied
                            var hasShape = !isRawPropertyName && expression.Arguments.Count > 1;

                            // translate the backlink property accessor
                            var property = TranslateExpression(expression.Arguments[0],
                                isRawPropertyName
                                    ? context.Enter(x => x.StringWithoutQuotes = true)
                                    : context.Enter(x => x.IncludeSelfReference = false));

                            var backlink = $".<{property}";

                            // if its a lambda, add the corresponding generic type as a [is x] statement
                            if (!isRawPropertyName)
                                backlink += $"[is {expression.Method.GetGenericArguments()[0].GetEdgeDBTypeName()}]";

                            // if it has a shape, translate the shape and add it to the backlink
                            if (hasShape)
                                backlink += $"{{ {TranslateExpression(expression.Arguments[1], context)} }}";

                            return backlink;
                        }
                    case nameof(QueryContext.SubQuery):
                        {
                            // pull the builder parameter and add it to a new lambda
                            // and execute it to get an instanc of the builder
                            var builder = (IQueryBuilder)Expression.Lambda(expression.Arguments[0]).Compile().DynamicInvoke()!;

                            // build it and copy its globals & parameters to our builder
                            var result = builder.BuildWithGlobals();

                            if (result.Parameters is not null)
                                foreach (var parameter in result.Parameters)
                                    context.SetVariable(parameter.Key, parameter.Value);

                            if (result.Globals is not null)
                                foreach (var global in result.Globals)
                                    context.SetGlobal(global.Name, global.Value, global.Reference);

                            // add the result as a sub query
                            return $"({result.Query})";
                        }
                    default:
                        throw new NotImplementedException($"{expression.Method.Name} does not have an implementation. This is a bug, please file a github issue with your query to reproduce this exception.");
                }
            }

            // check if our method translators can translate it
            if (MethodTranslator.TryTranslateMethod(expression, context, out var translatedMethod))
                return translatedMethod;

            // check if the method has an 'EquivalentOperator' attribute
            var edgeqlOperator = expression.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;

            if (edgeqlOperator != null)
            {
                // parse the parameters
                var argsArray = new object[expression.Arguments.Count];
                var parameters = expression.Method.GetParameters();
                for (int i = 0; i != argsArray.Length; i++)
                {
                    var arg = expression.Arguments[i];

                    // check if the argument is a query builder
                    if (parameters[i].ParameterType.IsAssignableTo(typeof(IQueryBuilder)))
                    {
                        // compile and execute the lambda to get an instance of the builder
                        var builder = (IQueryBuilder)Expression.Lambda(arg).Compile().DynamicInvoke()!;

                        // build it and copy its parameters to our builder, globals shoudln't be added here
                        var result = builder.BuildWithGlobals(node =>
                        {
                            // TODO: better checking on when shapes are required
                            switch (node)
                            {
                                case SelectNode select:
                                    select.Context.IncludeShape = false;
                                    break;
                            }
                        });

                        if (result.Globals?.Any() ?? false)
                            throw new NotSupportedException("Cannot use queries with parameters or globals within a sub-query expression");

                        if (result.Parameters is not null)
                            foreach (var parameter in result.Parameters)
                                context.SetVariable(parameter.Key, parameter.Value);

                        argsArray[i] = context.GetOrAddGlobal(builder, new SubQuery($"({result.Query})"));
                    }
                    else
                        argsArray[i] = TranslateExpression(arg, context);
                }

                // check if the edgeql operator has an initialization operator
                context.HasInitializationOperator = edgeqlOperator switch
                {
                    LinksAddLink or LinksRemoveLink => true,
                    _ => false
                };

                // build the operator
                return edgeqlOperator.Build(argsArray);
            }

            throw new Exception($"Couldn't find translator for {expression.Method.Name}");
        }
    }
}
