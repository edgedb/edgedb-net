using System.Runtime.CompilerServices;

namespace EdgeDB;

internal sealed class LastNodeObserver : INodeObserver, IDisposable
{
    public bool HasValue
        => !_resultBox.IsNull;

    public ref LooseLinkedList<Value>.Node Value
        => ref _resultBox.Value;


    private readonly RefBox<LooseLinkedList<Value>.Node> _resultBox = RefBox<LooseLinkedList<Value>.Node>.Null;

    private readonly QueryWriter _writer;

    public LastNodeObserver(QueryWriter writer)
    {
        _writer = writer;
        _writer.AddObserver(this);
    }

    public void OnAdd(ref LooseLinkedList<Value>.Node node)
    {
        _resultBox.Set(ref node);
    }

    public void OnRemove(ref LooseLinkedList<Value>.Node node)
    {
        unsafe
        {
            if(_resultBox.Pointer == Unsafe.AsPointer(ref node))
                _resultBox.Set(ref Unsafe.NullRef<LooseLinkedList<Value>.Node>());
        }
    }

    public void Dispose()
    {
        _writer.RemoveObserver(this);
    }
}
