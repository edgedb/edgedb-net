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
        public override string? Translate(MethodCallExpression expression, ExpressionContext context)
        {
            // figure out if the method is something we should translate or somthing that we should
            // call to pull the result from.
            if(ShouldTranslate(expression, context))
                return TranslateToEdgeQL(expression, context);

            // invoke and translate the result
            var result = Expression.Lambda(expression, context.RootExpression.Parameters).Compile().DynamicInvoke();

            // attempt to get the scalar type of the result of the method.
            if (!EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var type))
                throw new InvalidOperationException($"Cannot use {expression.Type} as a result in an un-translated context");

            // return the variable name containing the result of the method.
            return $"<{type}>{context.AddVariable(result)}";
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

        private string? TranslateToEdgeQL(MethodCallExpression expression, ExpressionContext context)
        {
            // if our method is within the query context class
            if (expression.Method.DeclaringType?.IsAssignableTo(typeof(IQueryContext)) ?? false)
            {
                switch (expression.Method.Name)
                {
                    case nameof(QueryContext.Global):
                        return TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true));
                    case nameof(QueryContext.Local):
                        {
                            // validate the type context, property should exist within the type.
                            var rawArg = TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true));
                            var rawPath = rawArg.Split('.');
                            string[] parsedPath = new string[rawPath.Length];

                            // build a path if the property is nested
                            for (int i = 0; i != rawPath.Length; i++)
                            {
                                var prop = (MemberInfo?)context.LocalScope?.GetProperty(rawPath[i]) ??
                                    context.LocalScope?.GetField(rawPath[i]) ??
                                    (MemberInfo?)context.NodeContext.CurrentType.GetProperty(rawPath[i]) ??
                                    context.NodeContext.CurrentType.GetField(rawPath[i]);

                                if (prop is null)
                                    throw new InvalidOperationException($"The property \"{rawPath[i]}\" within \"{rawArg}\" is out of scope");

                                parsedPath[i] = prop.GetEdgeDBPropertyName();
                            }

                            return $".{string.Join('.', parsedPath)}";
                        }
                    case nameof(QueryContext.UnsafeLocal):
                        {
                            // same thing as local except we dont validate anything here
                            return $".{TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true))}";
                        }
                    case nameof(QueryContext.Raw):
                        {
                            // return the raw string as a constant expression and serialize it without quotes
                            return TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true));
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

            // check if its a known method 
            if (EdgeQL.FunctionOperators.TryGetValue($"{expression.Method.DeclaringType?.Name}.{expression.Method.Name}", out edgeqlOperator))
            {
                var args = (expression.Object != null ? new string[] { TranslateExpression(expression.Object, context) } : Array.Empty<string>()).Concat(expression.Arguments.Select(x => TranslateExpression(x, context)));
                return edgeqlOperator.Build(args.ToArray());
            }

            throw new Exception($"Couldn't find translator for {expression.Method.Name}");
        }
    }
}
