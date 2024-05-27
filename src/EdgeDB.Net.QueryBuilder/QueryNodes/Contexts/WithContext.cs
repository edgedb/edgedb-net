using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents context for a <see cref="WithContext" />.
/// </summary>
internal class WithContext : NodeContext
{
    /// <inheritdoc />
    public WithContext(Type currentType) : base(currentType)
    {
    }

    /// <summary>
    ///     Gets the global variables that are included in the 'WITH' statement.
    /// </summary>
    public List<QueryGlobal>? Values { get; set; }

    public LambdaExpression? ValuesExpression { get; init; }
}
