namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents the context for a <see cref="DeleteNode" />.
/// </summary>
internal class DeleteContext : SelectContext
{
    /// <inheritdoc />
    public DeleteContext(Type currentType) : base(currentType)
    {
    }
}
