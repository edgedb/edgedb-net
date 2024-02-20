using ValueNode = EdgeDB.LooseLinkedList<EdgeDB.Value>.Node;

namespace EdgeDB;

internal sealed class ValueSpan : IDisposable
{
    private readonly List<Value> _nodes;

    private readonly QueryWriter _writer;

    public ValueSpan(QueryWriter writer)
    {
        _nodes = [];
        _writer = writer;
        writer.AddObserver(this);
    }

    public void OnAdd(ref ValueNode node)
    {
        _nodes.Add(node.Value);
    }

    public void OnRemove(ref ValueNode node)
    {
        _nodes.Remove(node.Value);
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }

    public Value[] ToTokens()
    {
        return _nodes.ToArray();
    }
}
