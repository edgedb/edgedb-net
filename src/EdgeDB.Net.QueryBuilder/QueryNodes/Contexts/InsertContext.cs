using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents context for a <see cref="InsertNode" />.
/// </summary>
internal class InsertContext : NodeContext
{
    /// <inheritdoc />
    public InsertContext(Type currentType, object? value) : base(currentType)
    {
        Value = value is not null
            ? Union<LambdaExpression, InsertNode.InsertValue, IJsonVariable>.From(value,
                () => InsertNode.InsertValue.FromType(currentType, value))
            : null;
    }

    /// <summary>
    ///     Gets the value that is to be inserted.
    /// </summary>
    public Union<LambdaExpression, InsertNode.InsertValue, IJsonVariable>? Value { get; init; }
}
