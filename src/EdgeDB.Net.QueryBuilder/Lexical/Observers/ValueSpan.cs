using ValueNode = EdgeDB.LooseLinkedList<EdgeDB.Value>.Node;

namespace EdgeDB;

internal sealed class ValueSpan : INodeObserver, IDisposable
{
    private readonly List<ValueNode> _nodes;

    private readonly QueryWriter _writer;

    public ValueSpan(QueryWriter writer)
    {
        _nodes = [];
        _writer = writer;
        writer.AddObserver(this);
    }

    public void OnAdd(ValueNode node)
    {
        _nodes.Add(node);
    }

    public void OnRemove(ValueNode node)
    {
        _nodes.Remove(node);
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }

    public Value[] ToTokens()
    {
        return _nodes.Select(x => x.Value).ToArray();
    }
}
