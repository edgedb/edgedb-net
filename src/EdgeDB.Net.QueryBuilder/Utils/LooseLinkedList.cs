using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
///     A non-circular linked list implementation.
/// </summary>
/// <typeparam name="T">The type to store in this linked list.</typeparam>
public sealed class LooseLinkedList<T> : IDisposable, IEnumerable<T>
{
    public delegate void NodeAction(Node? node);

    /// <summary>
    ///     Represents a node inside of the current linked list.
    /// </summary>
    [DebuggerDisplay("{DebugDisplay}"), StructLayout(LayoutKind.Sequential)]
    public sealed class Node(T value, LooseLinkedList<T> list)
    {
        [DebuggerHidden]
        private string DebugDisplay
            => $"Value({Value}) HasNext={Next is not null} HasPrevious={Previous is not null}";

        /// <summary>
        ///     Gets the next node within the list.
        /// </summary>
        public Node? Next { get; private set; }

        /// <summary>
        ///     Gets the previous node within the list.
        /// </summary>
        public Node? Previous { get; private set; }

        /// <summary>
        ///     The list that owns this node.
        /// </summary>
        internal LooseLinkedList<T> List = list;

        /// <summary>
        ///     The value within the node.
        /// </summary>
        public T Value { get; private set; } = value;

        internal void SetNext(Node? head)
            => Next = head;

        internal void SetPrevious(Node? previous)
            => Previous = previous;

        internal bool IsAlive { get; private set; } = true;

        public void Destroy()
        {
            if (!IsAlive) return;
            IsAlive = false;

            Next = null;
            Value = default!;
            Previous = null;
            List = null!;
        }
    }

    public sealed class NodeSlice(Node? head, Node? tail)
    {
        public static readonly NodeSlice Empty = new(null, null);

        public static NodeSlice Create(Node? head, Node? tail)
        {
            if (head is null && tail is null)
                return Empty;

            return new(head, tail);
        }

        public Node? Head { get; set; } = head;
        public Node? Tail { get; set; } = tail;
    }

    /// <summary>
    ///     Gets the number of elements within this list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets the first node in this list.
    /// </summary>
    public Node? First { get; private set; }

    /// <summary>
    ///     Gets the last node in this list.
    /// </summary>
    public Node? Last { get; private set; }

    public NodeSlice Slice(Node head, Node tail)
    {
        ValidateNode(head);
        ValidateNode(tail);
        return new NodeSlice(head, tail);
    }

    public NodeSlice Slice(Node head, int size)
    {
        ValidateNode(head);

        var tail = head;
        for (var i = 0; i != size && head.Next is not null; i++)
            tail = head.Next;

        return new NodeSlice(head, tail);
    }

    public void MoveSlice(NodeSlice slice, Node? target)
    {
        ValidateNodeSlice(slice);
        ValidateNode(target);

        // soft-delete the slice
        slice.Head?.Previous?.SetNext(slice.Tail?.Next);
        slice.Tail?.Next?.SetPrevious(slice.Head?.Previous);

        // rejoin the slice at the target
        target?.Next?.SetPrevious(slice.Tail);
        slice.Tail?.SetNext(target?.Next);
        target?.SetNext(slice.Head);
        slice.Head?.SetPrevious(target);
    }

    public int RemoveSlice(NodeSlice slice)
    {
        ValidateNodeSlice(slice);

        slice.Head?.Previous?.SetNext(slice.Tail?.Next);
        slice.Tail?.Next?.SetPrevious(slice.Head?.Previous);

        if (First == slice.Head)
            First = slice.Tail?.Next;

        if (Last == slice.Tail)
            Last = slice.Head?.Previous;

        var current = slice.Head;

        var delta = Count;
        while (current != slice.Tail && current is not null)
        {
            var next = current.Next;

            current.Destroy();

            current = next;
            Count--;
        }

        if (slice.Tail is not null && slice.Tail.IsAlive)
        {
            slice.Tail.Destroy();
            Count--;
        }

        return delta - Count;
    }

    /// <summary>
    ///     Adds a value after a specified node.
    /// </summary>
    /// <param name="node">The node to add the value after.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added values' node.</returns>
    public Node AddAfter(Node node, in T value)
    {
        ValidateNode(node);

        var newNode = CreateNode(in value);
        InsertNodeAfter(node, newNode);

        return newNode;
    }

    /// <summary>
    ///     Adds a node after a specified node.
    /// </summary>
    /// <param name="node">The node to add the new node after.</param>
    /// <param name="newNode">The new node to add.</param>
    public void AddAfter(Node node, Node newNode)
    {
        ValidateNode(node);
        ValidateNode(newNode);

        InsertNodeAfter(node, newNode);
    }

    /// <summary>
    ///     Adds a value before a specified node.
    /// </summary>
    /// <param name="node">The node to add the value before.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node.</returns>
    public Node AddBefore(Node node, in T value)
    {
        ValidateNode(node);

        var newNode = CreateNode(in value);
        InsertNodeBefore(node, newNode);

        return newNode;
    }

    /// <summary>
    ///     Adds a node before a specified node.
    /// </summary>
    /// <param name="node">The node to add the value before.</param>
    /// <param name="newNode">the new node to add.</param>
    public void AddBefore(Node node, Node newNode)
    {
        ValidateNode(node);
        ValidateNode(newNode);

        InsertNodeBefore(node, newNode);
    }

    /// <summary>
    ///     Adds a value to the front of this list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node.</returns>
    public Node AddFirst(in T value)
    {
        var newNode = CreateNode(in value);

        if (First is null)
        {
            InsertNodeEmpty(newNode);
        }
        else
        {
            InsertNodeBefore(First, newNode);
        }

        return newNode;
    }

    /// <summary>
    ///     Adds a node to the front of the list.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddFirst(Node node)
    {
        ValidateNode(node);

        if (First is null)
        {
            InsertNodeEmpty(node);
        }
        else
        {
            InsertNodeBefore(First, node);
        }
    }

    /// <summary>
    ///     Adds a value to the end of the list.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The newly added node.</returns>
    public Node AddLast(in T value)
    {
        var node = CreateNode(in value);

        if (Last is null)
        {
            InsertNodeEmpty(node);
        }
        else
        {
            InsertNodeAfter(Last, node);
        }

        return node;
    }

    /// <summary>
    ///     Adds a node to the end of the list.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddLast(Node node)
    {
        ValidateNode(node);

        if (Last is null)
        {
            InsertNodeEmpty(node);
        }
        else
        {
            InsertNodeAfter(Last, node);
        }
    }

    /// <summary>
    ///     Removes every value within this list.
    /// </summary>
    public void Clear()
    {
        var current = First;

        while (current is not null)
        {
            var temp = current;
            current = current.Next;
            temp.Destroy();
        }

        First = null;
        Last = null;
        Count = 0;
    }

    /// <summary>
    ///     Finds a specific value within this list.
    /// </summary>
    /// <remarks>
    ///     The result of this method can be null.
    /// </remarks>
    /// <param name="value">The value to find.</param>
    /// <returns>
    ///     The node containing the provided value if found; otherwise null.
    /// </returns>
    public Node? Find(in T value)
    {
        var node = First;
        var comparer = EqualityComparer<T>.Default;

        if (node is null)
            return node;

        if (value is null)
        {
            do
            {
                if (node.Value is null)
                    return node;
                node = node.Next;
            } while (node is not null);
        }
        else
        {
            do
            {
                if (comparer.Equals(node.Value, value))
                    return node;
                node = node.Next;
            } while (node is not null);
        }

        return null;
    }

    /// <summary>
    ///     Removes the specific value from the list.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns><c>true</c> if the value was removed; otherwise <c>false</c>.</returns>
    public bool Remove(in T value)
    {
        var node = Find(in value);

        if (node is null) return false;

        Remove(node);
        return true;

    }

    /// <summary>
    ///     Removes the specified node from the list.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void Remove(Node node)
    {
        ValidateNode(node);

        node.Next?.SetPrevious(node.Previous);
        node.Previous?.SetNext(node.Next);

        if (First == node)
        {
            First = node.Next;
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
    public void Remove(Node node, int count, NodeAction? action = null)
    {
        ValidateNode(node);
        var head = node.Previous;

        var current = node;
        for (var i = 0; i != count - 1 && current is not null; i++)
        {
            var next = current.Next;
            action?.Invoke(current);
            current.Destroy();
            current = next;
            Count--;
        }

        // the span of nodes removed has a head and tail
        if (current is not null && head is not null)
        {
            var tail = current.Next;
            action?.Invoke(current);
            current.Destroy();
            Count--;

            head.SetNext(tail);
            tail?.SetPrevious(head);
        }
        else if (current is null && head is not null)
        {
            // we've removed to the end of the list, 'head' becomes the tail
            head.SetNext(null);
            Last = head;
        }
        else if (head is null && current is not null)
        {
            // we've removed from the start of the list
            var tail = current.Next;
            action?.Invoke(current);
            current.Destroy();
            First = tail;
            Count--;
        }
        else
        {
            // both head and current is null, meaning we've removed the entire list, ensure head and tail is null and
            // set count to 0
            First = null;
            Last = null;

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
        if (First is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(First);
    }

    /// <summary>
    ///     Removes the last value within the list.
    /// </summary>
    /// <exception cref="InvalidOperationException">The list is empty.</exception>
    public void RemoveLast()
    {
        if (Last is null)
            throw new InvalidOperationException("Cannot remove from an empty list");
        Remove(Last);
    }

    /// <summary>
    ///     Determines if the provided node is valid to perform operations on.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <exception cref="ArgumentNullException">The node is null.</exception>
    /// <exception cref="InvalidOperationException">The node isn't apart of this list.</exception>
    private void ValidateNode(Node? node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        if (node.List != this)
            throw new InvalidOperationException("The provided node isn't apart of the list");
    }

    private void ValidateNodeSlice(NodeSlice? slice)
    {
        if (slice is null)
            throw new ArgumentNullException(nameof(slice));

        ValidateNode(slice.Head);
        ValidateNode(slice.Tail);
    }

    /// <summary>
    ///     Inserts a new node into this empty list.
    /// </summary>
    /// <param name="node">The node to insert.</param>
    private void InsertNodeEmpty(Node node)
    {
        First = node;
        Last = node;
        Count++;
    }

    /// <summary>
    ///     Inserts a node before another.
    /// </summary>
    /// <param name="node">The node to use as an anchor point.</param>
    /// <param name="newNode">The node to insert before the anchor point.</param>
    private void InsertNodeBefore(Node node, Node newNode)
    {
        node.Previous?.SetNext(newNode);
        newNode.SetPrevious(node.Previous);

        newNode.SetNext(node);
        node.SetPrevious(newNode);

        // change head if we're inserting before it.
        if (First == node)
            First = newNode;

        Count++;
    }

    /// <summary>
    ///     Inserts a node after another.
    /// </summary>
    /// <param name="node">The node to use as an anchor point.</param>
    /// <param name="newNode">The node to insert after the anchor point.</param>
    private void InsertNodeAfter(Node node, Node newNode)
    {
        node.Next?.SetPrevious(newNode);
        newNode.SetNext(node.Next);

        newNode.SetPrevious(node);
        node.SetNext(newNode);

        if (Last == node)
            Last = newNode;

        Count++;
    }

    /// <summary>
    ///     Allocates and initializes a new node
    /// </summary>
    /// <param name="value">The value to wrap the node around.</param>
    /// <returns>The newly created node.</returns>
    private Node CreateNode(in T value)
        => new(value, this);

    /// <summary>
    ///     Clears all nodes within this list.
    /// </summary>
    public void Dispose()
    {
        Clear();
    }

    public struct Enumerator : IEnumerator<T>
    {
        public T Current => _current!;

        private readonly LooseLinkedList<T> _list;
        private Node? _node;
        private T? _current;
        private int _index;

        internal Enumerator(LooseLinkedList<T> list)
        {
            _list = list;
            _node = list.First;
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
            _current = _node.Value;
            _node = _node.Next;

            return true;
        }

        void IEnumerator.Reset()
        {
            _current = default;
            _node = _list.First;
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
