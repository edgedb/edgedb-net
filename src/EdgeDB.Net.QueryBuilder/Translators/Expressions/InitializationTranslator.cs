using EdgeDB.QueryNodes;
using EdgeDB.Schema.DataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal static class InitializationTranslator
    {
        public static Dictionary<MemberInfo, Expression> PullInitializationExpression(Expression expression)
        {
            if(expression is MemberInitExpression memberInit)
            {
                return memberInit.Bindings.ToDictionary(
                    x => x.Member,
                    x => x is not MemberAssignment assignment
                        ? throw new InvalidOperationException($"Expected MemberAssignment, but got {x.GetType().Name}")
                        : assignment.Expression
                );
            }
            else if (expression is NewExpression newExpression)
            {
                if (newExpression.Members is null)
                    throw new NullReferenceException("New expression must contain arguments");

                return newExpression.Members.Zip(newExpression.Arguments).ToDictionary(x => x.First, x => x.Second);
            }

            throw new ArgumentException($"expression is not an initialization expression", nameof(expression));
        }

        private static SubQuery? GenerateMultiLinkInserter(Type innerType, IEnumerable collection, ExpressionContext context)
        {
            var methodDef = typeof(InitializationTranslator)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(x => x.ContainsGenericParameters && x.Name == nameof(GenerateMultiLinkInserter));

            if (methodDef is null)
                throw new NotSupportedException("Unable to find GenerateMultiLinkInserter<T>. this is a bug");

            return (SubQuery?)methodDef.MakeGenericMethod(innerType).Invoke(null, new object[] { collection, context });
        }
        private static SubQuery? GenerateMultiLinkInserter<T>(IEnumerable<T> collection, ExpressionContext context)
        {
            if (collection is null || !collection.Any())
                return null;

            return new SubQuery((info, subQuery) =>
            {
                var builder = new QueryBuilder<T>(info);

                var builtQuery = builder
                    .For(collection, x => QueryBuilder
                        .Insert(x)
                        .UnlessConflict()
                    )
                    .BuildWithGlobals();

                if(builtQuery.Parameters is not null)
                {
                    foreach (var param in builtQuery.Parameters)
                    {
                        context.SetVariable(param.Key, param.Value);
                    }
                }

                if(builtQuery.Globals is not null)
                {
                    foreach(var global in builtQuery.Globals)
                    {
                        context.SetGlobal(global.Name, global.Value, global.Reference);
                    }
                }

                subQuery.Append(builtQuery.Query);
            });
        }

        private static void CompileInsertExpression(
            Type propertyType,
            StringBuilder subQuery,
            TranslatorProxy shapeProxy,
            ObjectType objectInfo,
            ExpressionContext context)
        {
            var typename = propertyType.GetEdgeDBTypeName();

            subQuery
                .Append("(insert ")
                .Append(typename)
                .Append(" { ");

            shapeProxy(subQuery);

            subQuery
                .Append(" } ")
                .Append(ConflictUtils.GenerateExclusiveConflictStatement(objectInfo, true))
                .Append(" else ( select ")
                .Append(typename)
                .Append("))");
        }

        private static void AppendInitialization(StringBuilder builder, string name, string? value = null)
        {
            builder
                .Append(name)
                .Append(" := ")
                .Append(value ?? "{}");
        }

        public static void Translate(
            List<(EdgeDBPropertyInfo, Expression)> expressions,
            ExpressionContext context,
            StringBuilder result)
        {
            // context.IsShape = true;
            // return $"{{ {string.Join(", ", initializations)} }}";

            result.Append("{ ");

            for (var i = 0; i != expressions.Count; i++)
            {
                var (property, expression) = expressions[i];

                // get the members type and edgedb equivalent name
                var isLink = EdgeDBTypeUtils.IsLink(property.Type, out var isMultiLink, out var innerType);
                var disassembled = ExpressionUtils.DisassembleExpression(expression).ToArray();

                switch (expression)
                {
                    case MemberExpression when isLink && isMultiLink:
                        {
                            if (disassembled.Last() is ConstantExpression constant && disassembled[^2] is MemberExpression constParent)
                            {
                                if (!property.Type.IsAssignableTo(typeof(IEnumerable)))
                                    throw new NotSupportedException($"cannot use {property.Type} as a multi link collection type; its not assignable to IEnumerable.");

                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                var subQuery = GenerateMultiLinkInserter(innerType!, (IEnumerable)memberValue!, context);

                                if (subQuery == null)
                                {
                                    AppendInitialization(result, property.EdgeDBName);
                                    break;
                                }

                                AppendInitialization(
                                    result,
                                    property.EdgeDBName,
                                    context.GetOrAddGlobal(memberValue, subQuery)
                                );
                            }
                        }
                        break;
                    case MemberExpression when isLink && !isMultiLink:
                        {

                            if (disassembled.Last() is ConstantExpression constant && disassembled[^2] is MemberExpression constParent)
                            {
                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                // check if its a global value we've already got a query for
                                if (context.TryGetGlobal(memberValue, out var global))
                                {
                                    AppendInitialization(result, property.EdgeDBName, global.Name);
                                    break;
                                }

                                // TODO: revisit references
                                // check if its a value returned in a previous query
                                //if (QueryObjectManager.TryGetObjectId(memberValue, out var id))
                                //{
                                //    var globalName = context.GetOrAddGlobal(id, id.SelectSubQuery(property.Type));
                                //    initializations.Add($"{property.EdgeDBName} := {globalName}");
                                //    break;
                                //}

                                // generate an insert or select based on its unique constraints.
                                var name = QueryUtils.GenerateRandomVariableName();
                                context.SetGlobal(name, new SubQuery((info, subQuery) =>
                                {
                                    if (!info.TryGetObjectInfo(property.Type, out var objInfo))
                                        throw new InvalidOperationException(
                                            $"No schema type found for {property.Type}"
                                        );

                                    CompileInsertExpression(
                                        property.Type,
                                        subQuery,
                                        str =>
                                        {
                                            ExpressionTranslator.ContextualTranslate(
                                                QueryGenerationUtils.GenerateInsertShapeExpression(memberValue, property.Type),
                                                context,
                                                str
                                            );
                                        },
                                        objInfo,
                                        context
                                    );
                                }), memberValue);

                                AppendInitialization(result, property.PropertyName, name);
                            }
                            else if (disassembled.Last().Type.IsAssignableTo(typeof(QueryContext)))
                            {
                                result
                                    .Append(property.EdgeDBName)
                                    .Append(" := ");

                                ExpressionTranslator.ContextualTranslate(expression, context, result);
                            }
                            else
                                throw new InvalidOperationException($"Cannot translate {expression}");
                        }
                        break;
                    case not null when property.CustomConverter is not null:
                        {
                            // get the value, convert it, and parameterize it
                            if (!EdgeDBTypeUtils.TryGetScalarType(property.CustomConverter.Target, out var scalar))
                                throw new ArgumentException($"Cannot resolve scalar type for {property.CustomConverter.Target}");

                            var expressionResult = Expression.Lambda(expression).Compile().DynamicInvoke();
                            var converted = property.CustomConverter.ConvertTo(expressionResult);
                            var varName = context.AddVariable(converted);

                            result
                                .Append(property.EdgeDBName)
                                .Append(" := ")
                                .QueryArgument(scalar.ToString(), varName);
                        }
                        break;
                    case MemberInitExpression or NewExpression:
                        {
                            var name = QueryUtils.GenerateRandomVariableName();
                            context.SetGlobal(name, new SubQuery((info, subQuery) =>
                            {
                                if (!info.TryGetObjectInfo(property.Type, out var objInfo))
                                    throw new InvalidOperationException($"No schema type found for {property.Type}");

                                CompileInsertExpression(
                                    property.Type,
                                    subQuery,
                                    str => ExpressionTranslator.ContextualTranslate(expression, context, str),
                                    objInfo,
                                    context
                                );
                            }), null);

                            AppendInitialization(result, property.EdgeDBName, name);
                        }
                        break;
                    default:
                        {
                            // translate the value and determine if were setting a value or referencing a value.
                            var newContext = context.Enter(x =>
                            {
                                x.LocalScope = property.Type;
                                x.IsShape = false;
                            });

                            bool isSetter = !(context.NodeContext is not InsertContext and not UpdateContext &&
                                context.NodeContext.CurrentType.GetProperty(property.PropertyName) != null &&
                                expression is not MethodCallExpression);

                            result.Append(property.EdgeDBName);

                            var position = result.Length;

                            ExpressionTranslator.ContextualTranslate(expression!, newContext, result);

                            if (position == result.Length) // part of shape: no actual content was added
                                break;

                            if (newContext.IsShape)
                            {
                                // add the start and end shape form.
                                result
                                    .Insert(position, ": {")
                                    .Append('}');
                            }
                            else if ((isSetter || context.IsFreeObject) && !newContext.HasInitializationOperator)
                                result.Insert(position, " := "); // add assignment.
                        }
                        break;
                }

                if (i + 1 != expressions.Count)
                    result.Append(", ");
            }

            result.Append('}');
        }
    }
}
