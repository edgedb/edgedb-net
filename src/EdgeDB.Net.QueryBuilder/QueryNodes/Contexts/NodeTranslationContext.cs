using System.Linq.Expressions;

namespace EdgeDB.QueryNodes.Contexts;

internal sealed class NodeTranslationContext
{
    private readonly List<QueryGlobal> _globals;
    private readonly QueryNode _node;
    private readonly List<QueryGlobal> _prevGlobals;

    public NodeTranslationContext(QueryNode node)
    {
        _globals = new List<QueryGlobal>(node.Builder.QueryGlobals);
        _prevGlobals = new List<QueryGlobal>(node.Builder.QueryGlobals);
        _node = node;
    }

    public ContextConsumer CreateContextConsumer(LambdaExpression expression) => new(SetTrackedReferences, _node,
        expression, _node.Builder.QueryVariables, _globals);

    private void SetTrackedReferences()
    {
        foreach (var addedGlobal in _globals.Except(_prevGlobals))
        {
            _node.ReferencedGlobals.AddRange(_globals);
            _node.Builder.QueryGlobals.Add(addedGlobal);
        }
    }
}

internal sealed class ContextConsumer : ExpressionContext, IDisposable
{
    private readonly Action _callback;

    public ContextConsumer(
        Action callback,
        QueryNode node,
        LambdaExpression rootExpression,
        IDictionary<string, object?> queryArguments,
        List<QueryGlobal> globals)
        : base(node.Context, rootExpression, queryArguments, globals, node)
    {
        _callback = callback;
    }

    public void Dispose() => _callback();
}
