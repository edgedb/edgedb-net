using EdgeDB.QueryNodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LinqExpression = System.Linq.Expressions.Expression;

namespace EdgeDB.Translators.Expressions
{
    internal static class InitializationTranslator
    {
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
            
            foreach(var (Property, Expression) in expressions)
            {
                // get the members type and edgedb equivalent name
                var isLink = EdgeDBTypeUtils.IsLink(Property.Type, out var isMultiLink, out var innerType);
                var disassembled = ExpressionUtils.DisassembleExpression(Expression).ToArray();
                
                switch (Expression)
                {
                    case MemberExpression when isLink && isMultiLink:
                        {
                            if (disassembled.Last() is ConstantExpression constant && disassembled[^2] is MemberExpression constParent)
                            {
                                if (!Property.Type.IsAssignableTo(typeof(IEnumerable)))
                                    throw new NotSupportedException($"cannot use {Property.Type} as a multi link collection type; its not assignable to IEnumerable.");

                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                var result = GenerateMultiLinkInserter(innerType!, (IEnumerable)memberValue!, context);

                                if (result == null)
                                {
                                    initializations.Add($"{Property.EdgeDBName} := {{}}");
                                    break;
                                }

                                var name = context.GetOrAddGlobal(memberValue, result);

                                initializations.Add($"{Property.EdgeDBName} := {name}");
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
                                    initializations.Add($"{Property.EdgeDBName} := {global.Name}");
                                    break;
                                }

                                // check if its a value returned in a previous query
                                if (QueryObjectManager.TryGetObjectId(memberValue, out var id))
                                {
                                    var globalName = context.GetOrAddGlobal(id, id.SelectSubQuery(Property.Type));
                                    initializations.Add($"{Property.EdgeDBName} := {globalName}");
                                    break;
                                }

                                // generate an insert or select based on its unique constraints.
                                var name = QueryUtils.GenerateRandomVariableName();
                                context.SetGlobal(name, new SubQuery((info) =>
                                {
                                    // generate an insert shape
                                    var insertShape = ExpressionTranslator.ContextualTranslate(QueryGenerationUtils.GenerateInsertShapeExpression(memberValue, Property.Type), context);

                                    if (!info.TryGetObjectInfo(Property.Type, out var objInfo))
                                        throw new InvalidOperationException($"No schema type found for {Property.Type}");

                                    var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {Property.Type.GetEdgeDBTypeName()})";
                                    return $"(insert {Property.Type.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                                }), null);
                                initializations.Add($"{Property.EdgeDBName} := {name}");
                            }
                            else if (disassembled.Last().Type.IsAssignableTo(typeof(QueryContext)))
                            {
                                var translated = ExpressionTranslator.ContextualTranslate(Expression, context);
                                initializations.Add($"{Property.EdgeDBName} := {translated}");
                            }
                            else
                                throw new InvalidOperationException($"Cannot translate {Expression}");
                        }
                        break;
                    case LinqExpression when Property.CustomConverter is not null:
                        {
                            // get the value, convert it, and parameterize it
                            if (!EdgeDBTypeUtils.TryGetScalarType(Property.CustomConverter.Target, out var scalar))
                                throw new ArgumentException($"Cannot resolve scalar type for {Property.CustomConverter.Target}");

                            var result = LinqExpression.Lambda(Expression).Compile().DynamicInvoke();
                            var converted = Property.CustomConverter.ConvertTo(result);
                            var varName = context.AddVariable(converted);
                            initializations.Add($"{Property.EdgeDBName} := <{scalar}>${varName}");
                        }
                        break;
                    case MemberInitExpression or NewExpression:
                        {
                            var name = QueryUtils.GenerateRandomVariableName();
                            var expression = Expression;
                            context.SetGlobal(name, new SubQuery((info) =>
                            {
                                // generate an insert shape
                                var insertShape = ExpressionTranslator.ContextualTranslate(expression, context);

                                if (!info.TryGetObjectInfo(Property.Type, out var objInfo))
                                    throw new InvalidOperationException($"No schema type found for {Property.Type}");

                                var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {Property.Type.GetEdgeDBTypeName()})";

                                return $"(insert {Property.Type.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                            }), null);
                            initializations.Add($"{Property.EdgeDBName} := {name}");
                        }
                        break;
                    default:
                        {
                            // translate the value and determine if were setting a value or referencing a value.
                            var newContext = context.Enter(x =>
                            {
                                x.LocalScope = Property.Type;
                                x.IsShape = false;
                            });
                            string? value = ExpressionTranslator.ContextualTranslate(Expression, newContext);
                            bool isSetter = !(context.NodeContext is not InsertContext and not UpdateContext && 
                                context.NodeContext.CurrentType.GetProperty(Property.PropertyName) != null && 
                                Expression is not MethodCallExpression);

                            // add it to our shape
                            if (value is null) // include
                                initializations.Add(Property.EdgeDBName);
                            else if (newContext.IsShape) // includelink
                                initializations.Add($"{Property.EdgeDBName}: {{ {value} }}");
                            else
                                initializations.Add($"{Property.EdgeDBName}{((isSetter || context.IsFreeObject) && !newContext.HasInitializationOperator ? " :=" : "")} {value}");
                        }
                        break;
                }
            }

            context.IsShape = true;
            return $"{{ {string.Join(", ", initializations)} }}";
        }
    }
}
