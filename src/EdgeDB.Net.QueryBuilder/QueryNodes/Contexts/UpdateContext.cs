using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents context for a <see cref="UpdateNode" />.
/// </summary>
internal class UpdateContext : NodeContext
{
    /// <inheritdoc />
    public UpdateContext(Type currentType) : base(currentType)
    {
    }

    /// <summary>
    ///     Gets the update factory used within the 'SET' statement.
    /// </summary>
    public LambdaExpression? UpdateExpression { get; init; }

    /// <summary>
    ///     Gets the selector for the target of the update statement.
    /// </summary>
    public LambdaExpression? Selector { get; init; }
}
