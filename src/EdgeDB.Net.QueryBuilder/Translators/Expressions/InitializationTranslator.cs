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
    internal static class InitializationTranslator
    {
        public static string? Translate(IDictionary<MemberInfo, Expression> expressions, ExpressionContext context)
        {
            List<string> initializations = new();
            
            foreach(var (Member, Expression) in expressions)
            {
                // get the members type and edgedb equivalent name
                var memberType = Member.GetMemberType();
                var memberName = Member.GetEdgeDBPropertyName();
                var typeName = memberType.GetEdgeDBTypeName();
                var isLink = EdgeDBTypeUtils.IsLink(memberType, out var isMultiLink, out var innerType);

                switch (Expression)
                {
                    case MemberExpression when isLink:
                        {
                            var disassembled = ExpressionUtils.DisassembleExpression(Expression).ToArray();
                            if (disassembled.Last() is ConstantExpression constant && disassembled[disassembled.Length - 2] is MemberExpression constParent)
                            {
                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                // check if its a global value we've alreay got a query for
                                if (context.TryGetGlobal(memberValue, out var global))
                                {
                                    initializations.Add($"{memberName} := {global.Name}");
                                    break;
                                }

                                // check if its a value returned in a previous query
                                if (QueryObjectManager.TryGetObjectId(memberValue, out var id))
                                {
                                    var globalName = context.GetOrAddGlobal(id, id.SelectSubQuery(memberType));
                                    initializations.Add($"{memberName} := {globalName}");
                                    break;
                                }

                                // generate an insert or select based on its unique constraints.
                                var name = QueryUtils.GenerateRandomVariableName();
                                context.SetGlobal(name, new SubQuery((info) =>
                                {
                                    // generate an insert shape
                                    var insertShape = ExpressionTranslator.ContextualTranslate(QueryGenerationUtils.GenerateInsertShapeExpression(memberValue, memberType), context);

                                    if (!info.TryGetObjectInfo(memberType, out var objInfo))
                                        throw new InvalidOperationException($"No schema type found for {memberType}");

                                    var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {typeName})";
                                    return $"(insert {memberType.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                                }), null);
                                initializations.Add($"{memberName} := {name}");
                            }
                            else if (disassembled.Last().Type.IsAssignableTo(typeof(QueryContext)))
                            {
                                var translated = ExpressionTranslator.ContextualTranslate(Expression, context);
                                initializations.Add($"{memberName} := {translated}");
                            }
                            else
                                throw new InvalidOperationException($"Cannot translate {Expression}");
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

                                if (!info.TryGetObjectInfo(memberType, out var objInfo))
                                    throw new InvalidOperationException($"No schema type found for {memberType}");

                                var exclusiveCondition = $"{ConflictUtils.GenerateExclusiveConflictStatement(objInfo, true)} else (select {typeName})";

                                return $"(insert {memberType.GetEdgeDBTypeName()} {{ {insertShape} }} {exclusiveCondition})";
                            }), null);
                            initializations.Add($"{memberName} := {name}");
                        }
                        break;
                    default:
                        {
                            // translate the value and determine if were setting a value or referencing a value.
                            var newContext = context.Enter(x =>
                            {
                                x.LocalScope = memberType;
                                x.IsShape = false;
                            });
                            string? value = ExpressionTranslator.ContextualTranslate(Expression, newContext);
                            bool isSetter = context.NodeContext is InsertContext or UpdateContext || 
                                context.NodeContext.CurrentType.GetProperty(Member.Name) == null || 
                                Expression is MethodCallExpression;

                            // add it to our shape
                            if (value is null) // include
                                initializations.Add(memberName);
                            else if (newContext.IsShape) // includelink
                                initializations.Add($"{memberName}: {{ {value} }}");
                            else
                                initializations.Add($"{memberName}{((isSetter || context.IsFreeObject) && !newContext.HasInitializationOperator ? " :=" : "")} {value}");
                        }
                        break;
                }
            }

            context.IsShape = true;
            return string.Join(", ", initializations);
        }
    }
}
