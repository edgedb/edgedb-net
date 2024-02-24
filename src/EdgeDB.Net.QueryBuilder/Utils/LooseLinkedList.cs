using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
///     A non-circular linked list implementation.
/// </summary>
/// <typeparam name="T">The type to store in this linked list.</typeparam>
public sealed unsafe class LooseLinkedList<T> : IDisposable, IEnumerable<T>
{
    public delegate void NodeAction(ref Node node);

    /// <summary>
    ///     Represents a node inside of the current linked list.
    /// </summary>
    [DebuggerDisplay("{DebugDisplay}")]
    public struct Node
    {
        [DebuggerHidden]
        private string DebugDisplay
            => $"Value({Value}) HasNext={NextPtr is not null} HasPrevious={PreviousPtr is not null}";

        /// <summary>
        ///     Gets the next node within the list.
        /// </summary>
        /// <remarks>
        ///     This value can be a by-ref null.
        /// </remarks>
        public ref Node Next
            => ref Unsafe.AsRef<Node>(NextPtr);

        /// <summary>
        ///     Gets the previous node within the list.
        /// </summary>
        /// <remarks>
        ///     This value can be a by-ref null.
        /// </remarks>
        public ref Node Previous
            => ref Unsafe.AsRef<Node>(PreviousPtr);

        /// <summary>
        ///     The pointer to the next node.
        /// </summary>
        internal Node* NextPtr;

        /// <summary>
        ///     The pointer to the previous node.
        /// </summary>
        internal Node* PreviousPtr;

        /// <summary>
        ///     The value within the node.
        /// </summary>
        public T Value;

        /// <summary>
        ///     The list that owns this node.
        /// </summary>
        internal LooseLinkedList<T> List;

        /// <summary>
        ///     Sets the next node of this node.
        /// </summary>
        /// <param name="next">The by-ref next node.</param>
        internal void SetNext(ref Node next)
            => NextPtr = (Node*)Unsafe.AsPointer(ref next);

        /// <summary>
        ///     Sets the previous node of this node.
        /// </summary>
        /// <param name="previous">The by-ref previous node.</param>
        internal void SetPrevious(ref Node previous)
            => PreviousPtr = (Node*)Unsafe.AsPointer(ref previous);

        /// <summary>
        ///     Determines if the provided by-ref node is <i>is the same reference</i> ; i.e. their pointer address
        ///     is the same.
        /// </summary>
        /// <param name="other">The other by-ref node to compare.</param>
        /// <returns><c>true</c> if the nodes' reference is equal to this ones; otherwise <c>false</c>.</returns>
        public bool ReferenceEquals(ref Node other)
            => Unsafe.AsPointer(ref this) == Unsafe.AsPointer(ref other);

        /// <summary>
        ///     Destroys this node, freeing its memory.
        /// </summary>
        internal void Destroy()
        {
            NextPtr = null;
            PreviousPtr = null;
            Value = default!;
            List = null!;

            Allocator.Free(ref this);
        }
    }

    /// <summary>
    ///     Gets the number of elements within this list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets the first node in this list as by-ref.
    /// </summary>
    public ref Node First
        => ref Unsafe.AsRef<Node>(_head);

    /// <summary>
    ///     Gets the last node in this list as by-ref.
    /// </summary>
    public ref Node Last
        => ref Unsafe.AsRef<Node>(_tail);

    private Node* _head;
    private Node* _tail;

    /// <summary>
    ///     Adds a value after a specified node.
    /// </summary>
    /// <param name="node">The node to add the value after.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added values' node as a by-ref value.</returns>
    public ref Node AddAfter(ref Node node, in T value)
    {
        ValidateNode(ref node);

        ref var newNode = ref CreateNode(in value);
        InsertNodeAfter(ref node, ref newNode);

        return ref newNode;
    }

    /// <summary>
    ///     Adds a node after a specified node.
    /// </summary>
    /// <param name="node">The node to add the new node after.</param>
    /// <param name="newNode">The new node to add.</param>
    public void AddAfter(ref Node node, ref Node newNode)
    {
        ValidateNode(ref node);
        ValidateNode(ref newNode);

        InsertNodeAfter(ref node, ref newNode);
    }

    /// <summary>
    ///     Adds a value before a specified node.
    /// </summary>
    /// <param name="node">The node to add the value before.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node as a by-ref value.</returns>
    public ref Node AddBefore(ref Node node, in T value)
    {
        ValidateNode(ref node);

        ref var newNode = ref CreateNode(in value);
        InsertNodeBefore(ref node, ref newNode);

        return ref newNode;
    }

    /// <summary>
    ///     Adds a node before a specified node.
    /// </summary>
    /// <param name="node">The node to add the value before.</param>
    /// <param name="newNode">the new node to add.</param>
    public void AddBefore(ref Node node, ref Node newNode)
    {
        ValidateNode(ref node);
        ValidateNode(ref newNode);

        InsertNodeBefore(ref node, ref newNode);
    }

    /// <summary>
    ///     Adds a value to the front of this list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node as a by-ref value.</returns>
    public ref Node AddFirst(in T value)
    {
        ref var newNode = ref CreateNode(in value);

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

    /// <summary>
    ///     Adds a node to the front of the list.
    /// </summary>
    /// <param name="node">The node to add.</param>
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

    /// <summary>
    ///     Adds a value to the end of the list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node as a by-ref value.</returns>
    public ref Node AddLast(in T value)
    {
        ref var node = ref CreateNode(in value);

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

    /// <summary>
    ///     Adds a node to the end of the list.
    /// </summary>
    /// <param name="node">The node to add.</param>
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

    /// <summary>
    ///     Removes every value within this list.
    /// </summary>
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

    /// <summary>
    ///     Finds a specific value within this list.
    /// </summary>
    /// <remarks>
    ///     The result of this method can be a by-ref null.
    /// </remarks>
    /// <param name="value">The value to find.</param>
    /// <returns>
    ///     The node containing the provided value as a by-ref value if found; otherwise a by-ref null.
    /// </returns>
    public ref Node Find(in T value)
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

    /// <summary>
    ///     Removes the specific value from the list.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns><c>true</c> if the value was removed; otherwise <c>false</c>.</returns>
    public bool Remove(in T value)
    {
        ref var node = ref Find(in value);

        if (Unsafe.IsNullRef(ref node)) return false;
        Remove(ref node);
        return true;

    }

    /// <summary>
    ///     Removes the specified by-ref node from the list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
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

    /// <summary>
    ///     Removes N nodes.
    /// </summary>
    /// <param name="node">The node to start from.</param>
    /// <param name="count">The number of nodes to remove.</param>
    /// <param name="action">A delegate that's called right before a node is removed.</param>
    public void Remove(ref Node node, int count, NodeAction? action = null)
    {
        ValidateNode(ref node);

        ref var head = ref node.Previous;

        ref var current = ref node;
        for (var i = 0; i != count - 1 && !Unsafe.IsNullRef(ref current); i++)
        {
            ref var next = ref current.Next;
            action?.Invoke(ref current);
            current.Destroy();
            current = ref next;
            Count--;
        }

        // the span of nodes removed has a head and tail
        if (!Unsafe.IsNullRef(ref current) && !Unsafe.IsNullRef(ref head))
        {
            ref var tail = ref current.Next;
            action?.Invoke(ref current);
            current.Destroy();
            Count--;

            head.SetNext(ref tail);
            tail.SetPrevious(ref head);
        }
        else if (Unsafe.IsNullRef(ref current) && !Unsafe.IsNullRef(ref head))
        {
            // we've removed to the end of the list, 'head' becomes the tail
            head.NextPtr = null;
            _tail = (Node*) Unsafe.AsPointer(ref head);
        }
        else if (Unsafe.IsNullRef(ref head) && !Unsafe.IsNullRef(ref current))
        {
            // we've removed from the start of the list
            ref var tail = ref current.Next;
            action?.Invoke(ref current);
            current.Destroy();
            _head = (Node*) Unsafe.AsPointer(ref tail);
            Count--;
        }
        else
        {
            // both head and current is null, meaning we've removed the entire list, ensure head and tail is null and
            // set count to 0
            if (_head is not null)
                _head = null;

            if (_tail is not null)
                _tail = null;

            if (Count != 0)
                Count = 0;
        }
    }

    /// <summary>
    ///     Removes the first value in the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The list is empty.</exception>
    public void RemoveFirst()
    {
        if (_head is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(ref First);
    }

    /// <summary>
    ///     Removes the last value within the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The list is empty.</exception>
    public void RemoveLast()
    {
        if (_tail is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(ref Last);
    }

    /// <summary>
    ///     Determines if the provided node is valid to perform operations on.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    /// <exception cref="InvalidOperationException">The node isn't apart of this list.</exception>
    private void ValidateNode(ref Node node)
    {
        if (Unsafe.IsNullRef(ref node))
            throw new ArgumentNullException(nameof(node));

        if (this != node.List)
            throw new InvalidOperationException("The provided node isn't apart of this list");
    }

    /// <summary>
    ///     Inserts a new node into this empty list.
    /// </summary>
    /// <param name="node"></param>
    private void InsertNodeEmpty(ref Node node)
    {
        _head = (Node*)Unsafe.AsPointer(ref node);
        _tail = _head;
        Count++;
    }

    /// <summary>
    ///     Inserts a node before another.
    /// </summary>
    /// <param name="node">The node to use as an anchor point.</param>
    /// <param name="newNode">The node to insert before the anchor point.</param>
    private void InsertNodeBefore(ref Node node, ref Node newNode)
    {
        if (!Unsafe.IsNullRef(ref node.Previous))
        {
            node.Previous.SetNext(ref newNode);
            newNode.SetPrevious(ref node.Previous);
        }

        newNode.SetNext(ref node);
        node.SetPrevious(ref newNode);

        // change head if we're inserting before it.
        if (_head == (Node*)Unsafe.AsPointer(ref node))
            _head = (Node*)Unsafe.AsPointer(ref newNode);

        Count++;
    }

    /// <summary>
    ///     Inserts a node after another.
    /// </summary>
    /// <param name="node">The node to use as an anchor point.</param>
    /// <param name="newNode">The node to insert after the anchor point.</param>
    private void InsertNodeAfter(ref Node node, ref Node newNode)
    {
        if (!Unsafe.IsNullRef(ref node.Next))
        {
            node.Next.SetPrevious(ref newNode);
            newNode.SetNext(ref node.Next);
        }

        newNode.SetPrevious(ref node);
        node.SetNext(ref newNode);

        if (_tail == (Node*)Unsafe.AsPointer(ref node))
            _tail = (Node*)Unsafe.AsPointer(ref newNode);

        Count++;
    }

    /// <summary>
    ///     Allocates and initializes a new node
    /// </summary>
    /// <param name="value">The value to wrap the node around.</param>
    /// <returns>The newly created node as a by-ref value.</returns>
    private ref Node CreateNode(in T value)
    {
        ref var node = ref Allocator.Allocate<Node>();
        node.Value = value;
        node.List = this!;
        node.NextPtr = null;
        node.PreviousPtr = null;
        return ref node;
    }

    /// <summary>
    ///     Clears and de-allocs all nodes within this list.
    /// </summary>
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
