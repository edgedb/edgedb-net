using EdgeDB.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal sealed class GroupNode(NodeBuilder builder) : QueryNode<GroupContext>(builder)
    {
        private WriterProxy? _by;
        private WriterProxy? _using;
        private string? _usingReferenceName;

        public override void FinalizeQuery(QueryWriter writer)
        {
            if (_by is null)
                throw new InvalidOperationException("A 'by' expression is required for groups!");

            writer.Append("group ");

            if (Context.Selector is not null)
            {
                if (_usingReferenceName is not null)
                {
                    writer.Append(_usingReferenceName, " := ");
                }

                writer.Append(ProxyExpression(Context.Selector));
            }
            else
            {
                writer.Append(OperatingType.GetEdgeDBTypeName());

                if (Context.IncludeShape)
                {
                    (Context.Shape ?? BaseShapeBuilder.CreateDefault(GetOperatingType()))
                        .GetShape()
                        .Compile(writer.Append(' '), (writer, expression) =>
                        {
                            using var consumer = NodeTranslationContext.CreateContextConsumer(expression.Root);
                            ExpressionTranslator.ContextualTranslate(expression.Expression, consumer, writer);
                        });
                }
            }

            _using?.Invoke(writer);
            _by?.Invoke(writer);
        }

        public void By(LambdaExpression selector)
        {
            _by ??= writer => writer.Append(" by ", ProxyExpression(selector));
        }

        public void Using(LambdaExpression expression)
        {
            if (Context.Selector?.Body is MemberExpression)
            {
                // selecting out a property, create an alias
                _usingReferenceName = QueryUtils.GenerateRandomVariableName();
            }

            _using ??= writer => writer.Append(" using ",
                ProxyExpression(expression, ctx =>
                {
                    ctx.WrapNewExpressionInBrackets = false;

                    if (_usingReferenceName is not null)
                    {
                        ctx.ParameterAliases.Add(expression.Parameters.First(), _usingReferenceName);
                    }
                    //ctx.UseInitializationOperator = true;
                }));
        }
    }
}
