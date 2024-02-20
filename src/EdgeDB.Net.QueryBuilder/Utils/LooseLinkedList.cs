using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
public sealed unsafe class LooseLinkedList<T> : IDisposable, IEnumerable<T>
{
    public struct Node
    {
        public ref Node Next
            => ref Unsafe.AsRef<Node>(NextPtr);

        public ref Node Previous
            => ref Unsafe.AsRef<Node>(PreviousPtr);


        internal Node* NextPtr;
        internal Node* PreviousPtr;

        public T Value;

        internal LooseLinkedList<T> List;

        internal void SetNext(ref Node next)
            => NextPtr = (Node*)Unsafe.AsPointer(ref next);

        internal void SetPrevious(ref Node previous)
            => PreviousPtr = (Node*)Unsafe.AsPointer(ref previous);

        public bool ReferenceEquals(ref Node other)
            => Unsafe.AsPointer(ref this) == Unsafe.AsPointer(ref other);

        internal void Destroy()
        {
            NextPtr = null;
            PreviousPtr = null;
            Value = default!;
            List = null!;

            RefList.Invalidate(Unsafe.AsPointer(ref this));
            NativeMemory.Free(Unsafe.AsPointer(ref this));
        }
    }

    public int Count { get; private set; }

    public ref Node First
        => ref Unsafe.AsRef<Node>(_head);

    public ref Node Last
        => ref Unsafe.AsRef<Node>(_tail);

    private Node* _head;
    private Node* _tail;

    public ref Node AddAfter(ref Node node, T value)
    {
        ValidateNode(ref node);
        ref var newNode = ref CreateNode(value);
        InsertNodeAfter(ref node, ref newNode);

        return ref newNode;
    }

    public void AddAfter(ref Node node, ref Node newNode)
    {
        ValidateNode(ref node);
        ValidateNode(ref newNode);

        InsertNodeAfter(ref node, ref newNode);
    }

    public ref Node AddBefore(ref Node node, T value)
    {
        ValidateNode(ref node);

        ref var newNode = ref CreateNode(value);
        InsertNodeBefore(ref node, ref newNode);

        return ref newNode;
    }

    public void AddBefore(ref Node node, ref Node newNode)
    {
        ValidateNode(ref node);
        ValidateNode(ref newNode);

        InsertNodeBefore(ref node, ref newNode);
    }

    public ref Node AddFirst(T value)
    {
        ref var newNode = ref CreateNode(value);

        if (_head is null)
        {
            InsertNodeEmpty(ref newNode);
        }
        else
        {
            InsertNodeBefore(ref First, ref newNode);
        }

        return ref newNode;
    }

    public void AddFirst(ref Node node)
    {
        ValidateNode(ref node);

        if (_head is null)
        {
            InsertNodeEmpty(ref node);
        }
        else
        {
            InsertNodeBefore(ref First, ref node);
        }
    }

    public ref Node AddLast(T value)
    {
        ref var node = ref CreateNode(value);

        if (_tail is null)
        {
            InsertNodeEmpty(ref node);
        }
        else
        {
            InsertNodeAfter(ref Last, ref node);
        }

        return ref node;
    }

    public void AddLast(ref Node node)
    {
        ValidateNode(ref node);

        if (_tail is null)
        {
            InsertNodeEmpty(ref node);
        }
        else
        {
            InsertNodeAfter(ref Last, ref node);
        }
    }


    public void Clear()
    {
        ref var current = ref First;

        while (!Unsafe.IsNullRef(ref current))
        {
            ref var temp = ref current;
            current = ref current.Next;
            temp.Destroy();
        }

        _head = null;
        Count = 0;
    }

    public ref Node Find(T value)
    {
        ref var node = ref First;
        var comparer = EqualityComparer<T>.Default;

        if (Unsafe.IsNullRef(ref node))
            return ref node;

        if (value is null)
        {
            do
            {
                if (node.Value is null)
                    return ref node;
                node = ref node.Next;
            } while (!Unsafe.IsNullRef(ref node));
        }
        else
        {
            do
            {
                if (comparer.Equals(node.Value, value))
                    return ref node;
                node = ref node.Next;
            } while (!Unsafe.IsNullRef(ref node));
        }

        return ref Unsafe.NullRef<Node>();
    }

    public bool Remove(T value)
    {
        ref var node = ref Find(value);

        if (!Unsafe.IsNullRef(ref node))
        {
            Remove(ref node);
            return true;
        }

        return false;
    }

    public void Remove(ref Node node)
    {
        ValidateNode(ref node);

        if (node.NextPtr is not null)
        {
            node.Next.SetPrevious(ref node.Previous);
        }

        if (node.PreviousPtr is not null)
        {
            node.Previous.SetNext(ref node.Next);
        }

        if (_head == (Node*)Unsafe.AsPointer(ref node))
        {
            _head = (Node*)Unsafe.AsPointer(ref node.Next);
        }

        node.Destroy();
        Count--;
    }

    public void RemoveFirst()
    {
        if (_head is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(ref First);
    }

    public void RemoveLast()
    {
        if (_tail is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(ref Last);
    }

    private void ValidateNode(ref Node node)
    {
        if (Unsafe.IsNullRef(ref node))
            throw new ArgumentNullException(nameof(node));

        if (node.List != this)
            throw new InvalidOperationException("The provided node isn't apart of this list");
    }

    private void InsertNodeEmpty(ref Node node)
    {
        _head = (Node*)Unsafe.AsPointer(ref node);
        _tail = _head;
        Count++;
    }

    private void InsertNodeBefore(ref Node node, ref Node newNode)
    {
        newNode.SetNext(ref node);
        node.SetPrevious(ref newNode);

        // change head if we're inserting before it.
        if (_head == (Node*)Unsafe.AsPointer(ref node))
            _head = (Node*)Unsafe.AsPointer(ref newNode);

        Count++;
    }

    private void InsertNodeAfter(ref Node node, ref Node newNode)
    {
        newNode.SetPrevious(ref node);
        node.SetNext(ref newNode);

        if (_tail == (Node*)Unsafe.AsPointer(ref node))
            _tail = (Node*)Unsafe.AsPointer(ref newNode);

        Count++;
    }

    private ref Node CreateNode(T value)
    {
        var node = AllocNode();
        node->Value = value;
        node->List = this!;
        node->NextPtr = null;
        node->PreviousPtr = null;
        return ref Unsafe.AsRef<Node>(node);
    }

    public Node* AllocNode()
        => (Node*)NativeMemory.Alloc((nuint)sizeof(Node));


    public void Dispose()
    {
        Clear();
    }

    public struct Enumerator : IEnumerator<T>
    {
        public T Current => _current!;

        private readonly LooseLinkedList<T> _list;
        private Node* _node;
        private T? _current;
        private int _index;

        internal Enumerator(LooseLinkedList<T> list)
        {
            _list = list;
            _node = list._head;
            _current = default;
            _index = 0;
        }

        public bool MoveNext()
        {
            if (_node is null)
            {
                _index = _list.Count + 1;
                return false;
            }

            _index++;
            _current = _node->Value;
            _node = _node->NextPtr!;

            return true;
        }

        void IEnumerator.Reset()
        {
            _current = default;
            _node = _list._head;
            _index = 0;
        }

        public void Dispose() { }

        object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _list.Count + 1)
                    throw new InvalidCastException("Cannot get element");

                return Current;
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
