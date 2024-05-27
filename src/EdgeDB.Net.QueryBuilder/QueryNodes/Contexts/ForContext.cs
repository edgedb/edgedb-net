using System.Collections;
using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents context for a <see cref="ForNode" />.
/// </summary>
internal class ForContext : NodeContext
{
    /// <inheritdoc />
    public ForContext(Type currentType) : base(currentType)
    {
    }

    /// <summary>
    ///     Gets the iteration expression used to build the 'UNION (...)' statement.
    /// </summary>
    public LambdaExpression? Expression { get; init; }

    /// <summary>
    ///     Gets the collection used within the 'FOR' statement.
    /// </summary>
    public IEnumerable? Set { get; init; }

    public LambdaExpression? SetExpression { get; init; }
}
