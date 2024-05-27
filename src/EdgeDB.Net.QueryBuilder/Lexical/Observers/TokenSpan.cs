using TokenNode = EdgeDB.LooseLinkedList<EdgeDB.Token>.Node;

namespace EdgeDB;

internal sealed class TokenSpan : INodeObserver, IDisposable
{
    private readonly List<TokenNode> _nodes;

    private readonly QueryWriter _writer;

    public TokenSpan(QueryWriter writer)
    {
        _nodes = [];
        _writer = writer;
        writer.AddObserver(this);
    }

    public void OnAdd(TokenNode node)
    {
        _nodes.Add(node);
    }

    public void OnRemove(TokenNode node)
    {
        _nodes.Remove(node);
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }

    public Token[] ToTokens()
    {
        return _nodes.Select(x => x.Value).ToArray();
    }
}
