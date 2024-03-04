using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EdgeDB;

internal sealed class LastNodeObserver : INodeObserver, IDisposable
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue
        => Value is not null;

    public LooseLinkedList<Value>.Node? Value { get; private set; }


    private readonly QueryWriter _writer;

    public LastNodeObserver(QueryWriter writer)
    {
        _writer = writer;
        _writer.AddObserver(this);
    }

    public void OnAdd(LooseLinkedList<Value>.Node node)
    {
        Value = node;
    }

    public void OnRemove(LooseLinkedList<Value>.Node node)
    {
        if (Value == node)
        {
            Value = null;
        }
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }
}
