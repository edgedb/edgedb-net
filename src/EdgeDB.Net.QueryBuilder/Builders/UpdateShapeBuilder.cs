using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB;

public sealed class UpdateShapeBuilder<T, U> : IUpdateShapeBuilder
    where U : IQueryContext
{
    private readonly struct ShapeElement(MemberInfo key, LambdaExpression value, ExpressionType type)
    {
        public void Write(QueryWriter writer, Action<QueryWriter, LambdaExpression> translator)
        {
            var op = type switch
            {
                ExpressionType.Assign => ":=",
                ExpressionType.AddAssign => "+=",
                ExpressionType.SubtractAssign => "-=",
                _ => throw new InvalidOperationException($"Unsupported operator \"{type}\"")
            };
            var key1 = key;
            var expression = value;

            writer.Term(
                TermType.BinaryOp,
                "update_shape_element",
                Defer.This(() => $"Operator {op} for update shape element on {key1.Name}"),
                metadata: new BinaryOpMetadata(type),
                Token.Of(writer =>
                    {
                        writer.Append(key1.GetEdgeDBPropertyName(), ' ', op, ' ');
                        translator(writer, expression);
                    }
                )
            );
        }
    }

    private readonly LinkedList<ShapeElement> _elements = [];

    private UpdateShapeBuilder(LinkedList<ShapeElement> elements)
    {
        _elements = elements;
    }

    public UpdateShapeBuilder()
    {

    }

    internal static IUpdateShapeBuilder FromInitExpression(LambdaExpression expression)
    {

        var elements = new LinkedList<ShapeElement>();

        switch (expression.Body)
        {
            case MemberInitExpression memberInit:
                for (var i = 0; i < memberInit.Bindings.Count; i++)
                {
                    switch (memberInit.Bindings[i])
                    {
                        case MemberAssignment assignment:
                            elements.AddLast(new ShapeElement(assignment.Member,
                                Expression.Lambda(assignment.Expression, false, expression.Parameters),
                                ExpressionType.Assign));
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unsupported initialization binding {memberInit.Bindings[i].GetType().Name}");
                    }
                }
                break;
            case NewExpression newExpression when newExpression.Type.IsAnonymousType():
                var members = newExpression.Type.GetProperties();
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    elements.AddLast(new ShapeElement(members[i],
                        Expression.Lambda(newExpression.Arguments[i], false, expression.Parameters),
                        ExpressionType.Assign));
                }
                break;
            default:
                throw new InvalidOperationException($"Unsupported shape initialization type {expression.GetType()}");
        }

        return new UpdateShapeBuilder<T, U>(elements);
    }

    public UpdateShapeBuilder<T, U> Set<V>(Expression<Func<T, V>> selector, Expression<Func<T, U, V>> value)
        => AddElement(selector, value, ExpressionType.Assign);
    public UpdateShapeBuilder<T, U> Set<V>(Expression<Func<T, V>> selector, Expression<Func<T, V>> value)
        => AddElement(selector, value, ExpressionType.Assign);

    public UpdateShapeBuilder<T, U> Add<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, U, V>> value)
        => AddElement(selector, value, ExpressionType.AddAssign);
    public UpdateShapeBuilder<T, U> Add<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, V>> value)
        => AddElement(selector, value, ExpressionType.AddAssign);

    public UpdateShapeBuilder<T, U> Add<V>(Expression<Func<T, IEnumerable<V>?>> selector,
        Expression<Func<T, IEnumerable<V>>> value)
        => AddElement(selector, value, ExpressionType.AddAssign);

    public UpdateShapeBuilder<T, U> Add<V>(Expression<Func<T, IEnumerable<V>?>> selector,
        Expression<Func<T, U, IEnumerable<V>>> value)
        => AddElement(selector, value, ExpressionType.AddAssign);

    public UpdateShapeBuilder<T, U> Remove<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, U, V>> value)
        => AddElement(selector, value, ExpressionType.SubtractAssign);
    public UpdateShapeBuilder<T, U> Remove<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, V>> value)
        => AddElement(selector, value, ExpressionType.SubtractAssign);

    public UpdateShapeBuilder<T, U> Remove<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, U, IEnumerable<V>>> value)
        => AddElement(selector, value, ExpressionType.SubtractAssign);
    public UpdateShapeBuilder<T, U> Remove<V>(Expression<Func<T, IEnumerable<V>?>> selector, Expression<Func<T, IEnumerable<V>>> value)
        => AddElement(selector, value, ExpressionType.SubtractAssign);

    private UpdateShapeBuilder<T, U> AddElement(LambdaExpression selector, LambdaExpression value, ExpressionType type)
    {
        var key = ExpressionUtils.GetMemberSelection(selector);

        _elements.AddLast(new ShapeElement(key, value, type));

        return this;
    }

    internal void Compile(QueryWriter writer, Action<QueryWriter, LambdaExpression> translator)
    {
        writer.Shape(
            "update_shape",
            _elements.ToArray(),
            (writer, v) => v.Write(writer, translator),
            debug: Defer.This(() => $"Update shape for {typeof(T).Name}")
        );
    }

    void IUpdateShapeBuilder.Compile(QueryWriter writer, Action<QueryWriter, LambdaExpression> translator)
        => Compile(writer, translator);
}

internal interface IUpdateShapeBuilder
{
    void Compile(QueryWriter writer, Action<QueryWriter, LambdaExpression> translator);
}
