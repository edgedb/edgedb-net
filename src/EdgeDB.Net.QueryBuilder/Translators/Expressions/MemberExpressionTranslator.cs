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
    ///     Represents a translator for translating an expression accessing a field or property.
    /// </summary>
    internal class MemberExpressionTranslator : ExpressionTranslator<MemberExpression>
    {
        private (Expression BaseExpression, MemberExpression[] Path) DisassembleExpression(MemberExpression member)
        {
            var path = new List<MemberExpression>() { member };
            var current = member.Expression;

            while (current is MemberExpression element)
            {
                path.Add(element);
                current = element.Expression;
            }

            return (
                current ?? path.Last(),
                path.ToArray()
            );
        }

        /// <inheritdoc/>
        public override void Translate(MemberExpression expression, ExpressionContext context, QueryWriter writer)
        {
            // deconstruct the member access tree.
            var (baseExpression, path) = DisassembleExpression(expression);

            if (PropertyTranslationTable.TryGetTranslator(path[0], out var translator))
            {
                translator(writer, path[0], context);
                return;
            }

            switch (baseExpression)
            {
                case { } contextParam when contextParam.Type.IsAssignableTo(typeof(IQueryContext)):
                    TranslateContextMember(writer, contextParam, path, context);
                    break;
                case ParameterExpression param:
                    TranslateParameterMember(writer, param, path, context);
                    break;
                case ConstantExpression constant:
                    TranslateConstantMember(writer, constant, path, context);
                    break;
            }
        }

        private void TranslateConstantMember(QueryWriter writer, ConstantExpression constant, MemberExpression[] path,
            ExpressionContext context)
        {
            if (!EdgeDBTypeUtils.TryGetScalarType(path[0].Type, out var edgeqlType))
                throw new NotSupportedException($"The type {path[0].Type} cannot be used as a query argument");

            var refHolder = constant.Value;

            for (var i = path.Length - 1; i >= 0; i--)
            {
                refHolder = path[i].Member.GetMemberValue(refHolder);
            }

            writer.QueryArgument(edgeqlType.ToString(), context.AddVariable(refHolder));
        }

        private void TranslateParameterMember(QueryWriter writer, ParameterExpression parameter, MemberExpression[] path, ExpressionContext context)
        {
            if (context.ParameterPrefixes.TryGetValue(parameter, out var prefix))
                writer.Append(prefix);
            else if (context.IncludeSelfReference)
                writer.Append('.');

            WritePath(writer, path);
        }

        private void TranslateContextMember(QueryWriter writer, Expression contextExpression, MemberExpression[] path, ExpressionContext context)
        {
            var contextAccessor = path[^1];

            switch (contextAccessor.Member.Name)
            {
                case nameof(IQueryContextSelf<object>.Self):
                case nameof(IQueryContextUsing<object>.Using):
                    WritePath(writer, path[..^1]);
                    break;
                case nameof(IQueryContextVars<object>.Variables):
                    var target = path[^2];
                    if (!context.TryGetGlobal(target.Member.Name, out var global))
                        throw new InvalidOperationException($"Unknown global in 'with' access of '{target.Member.Name}'");

                    context.Node?.ReferencedGlobals.Add(global);

                    if (target.Type.IsAssignableTo(typeof(IJsonVariable)))
                    {
                        // we go up to three because the real access looks like `ctx.Self.Value.XXXX.YY.ZZ...`
                        var jsonPath = path[..^3];

                        if (global.Reference is not IJsonVariable)
                            throw new InvalidOperationException($"The global '{global.Name}' is not a json value");

                        if (!EdgeDBTypeUtils.TryGetScalarType(path[0].Type, out var edgeqlType))
                            throw new InvalidOperationException("The json value must be a scalar type");

                        var args = new Terms.FunctionArg[jsonPath.Length + 1];
                        args[0] = new Terms.FunctionArg(global.Name);

                        for (var i = jsonPath.Length - 1; i >= 0; i--)
                        {
                            var pathRef = jsonPath[i].Member.Name;
                            args[i + 1] = Value.Of(writer => writer.SingleQuoted(pathRef));
                        }

                        writer
                            .TypeCast(edgeqlType.ToString())
                            .Function(
                                "json_get",
                                args
                            );
                        return;
                    }

                    writer.Marker(
                        MarkerType.GlobalReference,
                        global.Name,
                        Defer.This(() => "Global referenced from member expression"),
                        new GlobalReferenceMetadata(global),
                        Value.Of(writer =>
                        {
                            writer.Append(global.Name);

                            if (path.Length < 3) return;

                            writer.Append('.');
                            WritePath(writer, path[..^2]);
                        })
                    );
                    break;
            }
        }

        private void WritePath(QueryWriter writer, MemberExpression[] path)
        {
            for (var i = path.Length - 1; i > 0; i--)
            {
                writer.Append(path[i].Member.GetEdgeDBPropertyName(), '.');
            }

            writer.Append(path[0].Member.GetEdgeDBPropertyName());
        }
    }
}
