using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents a 'UPDATE' node.
/// </summary>
internal class UpdateNode : QueryNode<UpdateContext>
{
    private WriterProxy? _filter;
    private WriterProxy? _set;

    /// <inheritdoc />
    public UpdateNode(NodeBuilder builder) : base(builder) { }

    /// <inheritdoc />
    public override void Visit()
    {
    }

    private void AppendUpdateStatement(QueryWriter writer)
    {
        // resolve and append the UPDATE ... statement
        writer.Append("update ");

        if (Context.Selector is not null)
            TranslateExpression(Context.Selector, writer);
        else
            writer.Append(OperatingType.GetEdgeDBTypeName());

        _filter?.Invoke(writer);
        _set?.Invoke(writer);
    }

    /// <inheritdoc />
    public override void FinalizeQuery(QueryWriter writer)
    {
        // if the builder wants this node to be a global
        if (Context is {SetAsGlobal: true, GlobalName: not null})
        {
            SetGlobal(Context.GlobalName, new SubQuery(writer => writer
                .Wrapped(AppendUpdateStatement)
            ), null);
        }
        else
            AppendUpdateStatement(writer);
    }

    /// <summary>
    ///     Adds a filter to the update node.
    /// </summary>
    /// <param name="filter">The filter predicate to add.</param>
    public void Filter(LambdaExpression filter) =>
        _filter ??= writer =>
        {
            writer.Append(" filter ", ProxyExpression(filter));
        };

    public void Set(IUpdateShapeBuilder setter) =>
        _set ??= writer =>
        {
            writer.Append(" set ");
            setter.Compile(writer, (writer, expression) => TranslateExpression(expression, writer));
        };
}
