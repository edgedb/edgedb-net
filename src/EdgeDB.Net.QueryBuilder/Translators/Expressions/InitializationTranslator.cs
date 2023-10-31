using EdgeDB.QueryNodes;
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

            return new SubQuery(info =>
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

                return builtQuery.Query;
            });
        }

        public static string? Translate(IDictionary<EdgeDBPropertyInfo, Expression> expressions, ExpressionContext context)
        {
            List<string> initializations = new();
            
            foreach(var (property, expression) in expressions)
            {
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

                                var result = GenerateMultiLinkInserter(innerType!, (IEnumerable)memberValue!, context);

                                if (result == null)
                                {
                                    initializations.Add($"{property.EdgeDBName} := {{}}");
                                    break;
                                }

                                var name = context.GetOrAddGlobal(memberValue, result);

                                initializations.Add($"{property.EdgeDBName} := {name}");
                            }
                        }
                        break;
                    case MemberExpression when isLink && !isMultiLink:
                        {
                            
                            if (disassembled.Last() is ConstantExpression constant && disassembled[^2] is MemberExpression constParent)
                            {
                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                // check if its a global value we've alreay got a query for
                                if (context.TryGetGlobal(memberValue, out var global))
                                {
                                    initializations.Add($"{property.EdgeDBName} := {global.Name}");
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
                                context.SetGlobal(name, new SubQuery((info) =>
                                {
                                    // generate an insert shape
                                    var insertShape = ExpressionTranslator.ContextualTranslate(QueryGenerationUtils.GenerateInsertShapeExpression(memberValue, property.Type), context);

                                    if (!info.TryGetObjectInfo(property.Type, out var objInfo))
                                        throw new InvalidOperationException($"No schema type found for {property.Type}");

                                    var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {property.Type.GetEdgeDBTypeName()})";
                                    return $"(insert {property.Type.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                                }), null);
                                initializations.Add($"{property.EdgeDBName} := {name}");
                            }
                            else if (disassembled.Last().Type.IsAssignableTo(typeof(QueryContext)))
                            {
                                var translated = ExpressionTranslator.ContextualTranslate(expression, context);
                                initializations.Add($"{property.EdgeDBName} := {translated}");
                            }
                            else
                                throw new InvalidOperationException($"Cannot translate {expression}");
                        }
                        break;
                    case Expression when property.CustomConverter is not null:
                        {
                            // get the value, convert it, and parameterize it
                            if (!EdgeDBTypeUtils.TryGetScalarType(property.CustomConverter.Target, out var scalar))
                                throw new ArgumentException($"Cannot resolve scalar type for {property.CustomConverter.Target}");

                            var result = Expression.Lambda(expression).Compile().DynamicInvoke();
                            var converted = property.CustomConverter.ConvertTo(result);
                            var varName = context.AddVariable(converted);
                            initializations.Add($"{property.EdgeDBName} := <{scalar}>${varName}");
                        }
                        break;
                    case MemberInitExpression or NewExpression:
                        {
                            var name = QueryUtils.GenerateRandomVariableName();
                            context.SetGlobal(name, new SubQuery((info) =>
                            {
                                // generate an insert shape
                                var insertShape = ExpressionTranslator.ContextualTranslate((Expression)expression, context);

                                if (!info.TryGetObjectInfo(property.Type, out var objInfo))
                                    throw new InvalidOperationException($"No schema type found for {property.Type}");

                                var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {property.Type.GetEdgeDBTypeName()})";

                                return $"(insert {property.Type.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                            }), null);
                            initializations.Add($"{property.EdgeDBName} := {name}");
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
                            string? value = ExpressionTranslator.ContextualTranslate(expression, newContext);
                            bool isSetter = !(context.NodeContext is not InsertContext and not UpdateContext && 
                                context.NodeContext.CurrentType.GetProperty(property.PropertyName) != null && 
                                expression is not MethodCallExpression);

                            // add it to our shape
                            if (value is null) // include
                                initializations.Add(property.EdgeDBName);
                            else if (newContext.IsShape) // includelink
                                initializations.Add($"{property.EdgeDBName}: {{ {value} }}");
                            else
                                initializations.Add($"{property.EdgeDBName}{((isSetter || context.IsFreeObject) && !newContext.HasInitializationOperator ? " :=" : "")} {value}");
                        }
                        break;
                }
            }

            context.IsShape = true;
            return $"{{ {string.Join(", ", initializations)} }}";
        }
    }
}
