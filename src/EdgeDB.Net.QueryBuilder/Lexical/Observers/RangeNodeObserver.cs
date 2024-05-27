using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EdgeDB;

internal sealed class RangeNodeObserver : INodeObserver, IDisposable
{
    [MemberNotNullWhen(true, nameof(First))]
    [MemberNotNullWhen(true, nameof(Last))]
    public bool HasValue
        => First is not null;

    public LooseLinkedList<Token>.Node? First { get; private set; }

    public LooseLinkedList<Token>.Node? Last { get; private set; }


    private readonly QueryWriter _writer;

    public RangeNodeObserver(QueryWriter writer)
    {
        _writer = writer;
        _writer.AddObserver(this);
    }

    public void OnAdd(LooseLinkedList<Token>.Node node)
    {
        Last = node;

        if (First is not null) return;

        First = node;
    }

    public void OnRemove(LooseLinkedList<Token>.Node node)
    {
        if (First == node)
        {
            First = null;
        }

        if (Last == node)
        {
            Last = null;
        }
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }
}
