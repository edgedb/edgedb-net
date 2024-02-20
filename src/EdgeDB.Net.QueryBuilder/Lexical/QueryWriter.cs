using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ValueNode = EdgeDB.LooseLinkedList<EdgeDB.Value>.Node;

namespace EdgeDB;

internal sealed class QueryWriter : IDisposable
{
    public readonly struct PositionalQueryWriter : IDisposable
    {
        public readonly QueryWriter Writer = new();

        private readonly QueryWriter _parent;
        private readonly int _position;
        private readonly RefBox<ValueNode> _from;

        public PositionalQueryWriter(QueryWriter parent, int position)
        {
            _parent = parent;
            _position = position;
            _from = new RefBox<ValueNode>(ref _parent.FastGetNodeFromIndex(_position));
        }

        public PositionalQueryWriter(QueryWriter parent, int position, ref ValueNode from)
        {
            _parent = parent;
            _position = position;
            _from = new RefBox<ValueNode>(ref from);
        }

        public unsafe void Dispose()
        {
            // copy markers and tokens
            if (Unsafe.IsNullRef(ref _from.Value))
                throw new IndexOutOfRangeException("Position is not within the span of tokens");

            var tokenSet = new Dictionary<IntPtr, IntPtr>();

            ref var currentNode = ref Writer._tokens.First;
            while (!Unsafe.IsNullRef(ref currentNode))
            {
                tokenSet.Add(
                    (IntPtr) Unsafe.AsPointer(ref currentNode),
                    (IntPtr) Unsafe.AsPointer(ref _parent._tokens.AddBefore(ref _from.Value, currentNode.Value))
                );

                currentNode = ref currentNode.Next;
            }

            foreach (var marker in Writer._markers)
            {
                if (!_parent._markers.TryGetValue(marker.Key, out var markers))
                    markers = _parent._markers[marker.Key] = new();

                foreach (var item in marker.Value)
                {
                    item.Start =
                        new RefBox<ValueNode>(
                            ref Unsafe.AsRef<ValueNode>(tokenSet[(IntPtr)item.Start.Pointer].ToPointer())
                        );

                    _parent._markersRef.Add(item);
                    markers.AddLast(item);
                }
            }

            Writer.Dispose();
            _parent.UpdateMarkers(_position, Writer._tokens.Count);
        }
    }

    public IReadOnlyDictionary<string, LinkedList<Marker>> Markers
        => _markers;

    private readonly List<Marker> _markersRef;

    private readonly LooseLinkedList<Value> _tokens;
    private readonly Dictionary<string, LinkedList<Marker>> _markers;

    private readonly List<ValueSpan> _observers = [];

    private int TailIndex => _tokens.Count - 1;

    public QueryWriter()
    {
        _tokens = new();
        _markers = new();
        _markersRef = new();
    }

    public PositionalQueryWriter CreatePositionalWriter(int position)
    {
        return new PositionalQueryWriter(this, position);
    }

    public PositionalQueryWriter CreatePositionalWriter(int position, ref ValueNode from)
    {
        return new PositionalQueryWriter(this, position, ref from);
    }

    public void AddObserver(ValueSpan observer)
        => _observers.Add(observer);

    public void RemoveObserver(ValueSpan observer)
        => _observers.Remove(observer);

    private void OnNodeAdd(ref ValueNode node)
    {
        foreach(var span in _observers)
            span.OnAdd(ref node);
    }

    private void OnNodeRemove(ref ValueNode node)
    {
        foreach (var span in _observers)
            span.OnRemove(ref node);
    }

    private int GetIndexOfNode(ref ValueNode node)
    {
        ref var current = ref _tokens.First;
        for (int i = 0; !Unsafe.IsNullRef(ref current); i++)
        {
            if (current.ReferenceEquals(ref node))
                return i;

            current = ref current.Next;
        }

        return -1;
    }

    private ref ValueNode Traverse(ref ValueNode from, int count, bool dir)
    {
        ref var node = ref from;
        while (count > 0 && !Unsafe.IsNullRef(ref node))
            node = dir ? node.Next : node.Previous;
        return ref node;
    }

    private ref ValueNode FastGetNodeFromIndex(int index)
    {
        // check for start/end node
        if (index == 0)
            return ref _tokens.First;

        if (index == TailIndex)
            return ref _tokens.Last;

        // get a distance we'll have to traverse in worst case scenario
        var normalizedDistance = Math.Min(TailIndex - index, index);

        // check if searching through markers will be faster:
        // binary search is log(n) but since we're searching for a range which can have jump distances, we'll add a
        // small weight which can be tuned later to represent the jump distance within the span.
        if (_markersRef.Count <= Math.Log(normalizedDistance) + normalizedDistance / 2.5)
        {
            // get a midpoint somewhere in the markers derived from the provided index
            var i = (int)Math.Floor(_markersRef.Count * (_tokens.Count / (float)index));
            var minJump = normalizedDistance;

            // binary search
            while (true)
            {
                var split = i / 2;

                if (split == 0 || split == i)
                    break;

                var delta = Math.Abs(_markersRef[i].Position - index);

                if (delta < minJump)
                {
                    minJump = delta;
                    i -= split;
                }
                else if (delta > minJump)
                {
                    i += split;
                }
                else
                {
                    // last iter was same as this one, we can exit
                    break;
                }
            }

            var marker = _markersRef[i];
            var distance = Math.Abs(marker.Position - index);

            if(distance <= normalizedDistance)
                return ref Traverse(ref marker.Start.Value, Math.Abs(marker.Position - index), index > marker.Position);
        }

        return ref normalizedDistance == index
            ? ref Traverse(ref _tokens.First!, index, true)
            : ref Traverse(ref _tokens.Last!, _tokens.Count - index, false);
    }

    private void UpdateMarkers(int position, int delta)
    {
        // TODO: optimize, maybe store markers in a flat list
        foreach (var label in _markers.Values.SelectMany(x => x).Where(x => x.Position <= position))
        {
            label.Update(delta);
        }
    }

    public bool TryGetMarker(string name, [MaybeNullWhen(false)] out LinkedList<Marker> markers)
        => _markers.TryGetValue(name, out markers);

    public QueryWriter Marker(MarkerType type, string name, Value value)
    {
        if (!_markers.TryGetValue(name, out var markers))
            _markers[name] = markers = new();

        Append(value, out var head);
        var marker = new Marker(type, this, 1, TailIndex, ref head);
        _markersRef.Add(marker);
        markers.AddLast(marker);
        return this;
    }

    public QueryWriter Marker(MarkerType type, string name)
        => Marker(type, name, name);

    public QueryWriter Marker(MarkerType type, string name, params Value[] values)
    {
        if (values.Length == 0)
            return this;

        if (!_markers.TryGetValue(name, out var markers))
            _markers[name] = markers = new();

        Append(out var head, values);

        var marker = new Marker(
            type,
            this,
            values.Length,
            TailIndex - (values.Length - 1),
            ref head
        );
        _markersRef.Add(marker);
        markers.AddLast(marker);
        return this;
    }

    public QueryWriter Remove(int index, ref ValueNode head, int count = 1)
    {
        ref var tail = ref count <= 2 ? ref Traverse(ref head, count, true) : ref FastGetNodeFromIndex(index + count);

        if(Unsafe.IsNullRef(ref tail))
            throw new ArgumentOutOfRangeException($"Range exceeds the length at {index}..{index + count}");

        RemoveValueSpan(ref head, ref tail);

        return this;
    }

    public QueryWriter Remove(ValueNode head, int count = 1)
    {
        ref var tail = ref Traverse(ref head, count, true);

        if (Unsafe.IsNullRef(ref tail))
            throw new ArgumentOutOfRangeException($"Range of {count} exceeds the length {_tokens.Count}");

        RemoveValueSpan(ref head, ref tail);

        return this;
    }

    public QueryWriter Remove(int index, int count = 1)
    {
        ref var head = ref FastGetNodeFromIndex(index);

        if (Unsafe.IsNullRef(ref head))
            throw new IndexOutOfRangeException($"No element at {index}");

        ref var tail = ref count <= 2 ? ref Traverse(ref head, count, true) : ref FastGetNodeFromIndex(index + count);

        if (Unsafe.IsNullRef(ref tail))
            throw new ArgumentOutOfRangeException($"Range exceeds the length at {index}..{index + count}");

        RemoveValueSpan(ref head, ref tail);

        return this;
    }

    private void RemoveValueSpan(ref ValueNode head, ref ValueNode tail)
    {
        if (Unsafe.IsNullRef(ref head.Previous) && Unsafe.IsNullRef(ref tail.Next))
        {
            ref var current = ref _tokens.First;
            while (!Unsafe.IsNullRef(ref current))
            {
                OnNodeRemove(ref current);
                current = ref current.Next;
            }
            _tokens.Clear();
            return;
        }
        else if (Unsafe.IsNullRef(ref head.Previous))
        {
            _tokens.AddFirst(ref tail.Next);
        }
        else if (Unsafe.IsNullRef(ref tail.Next))
        {
            _tokens.AddLast(ref head.Previous);
        }

        ref var node = ref head;
        while (!Unsafe.IsNullRef(ref node))
        {
            ref var tmp = ref node;
            OnNodeRemove(ref node);
            node = ref node.Next;

            tmp.Destroy();
        }
    }

    public QueryWriter Replace(ref ValueNode node, int position, int size, Value value)
    {
        OnNodeAdd(ref _tokens.AddBefore(ref node, value));
        Remove(position, ref node, size);
        return this;
    }

    public QueryWriter Prepend(ref ValueNode node, Value value)
    {
        var index = GetIndexOfNode(ref node);

        if (index == -1)
            throw new InvalidOperationException("Node cannot be found in collection of tokens");

        OnNodeAdd(ref _tokens.AddBefore(ref node, value));
        UpdateMarkers(index - 1, 1);

        return this;
    }

    public QueryWriter Append(Value value, out ValueNode node)
    {
        node = _tokens.AddLast(value);
        OnNodeAdd(ref node);
        UpdateMarkers(TailIndex, 1);

        return this;
    }

    public QueryWriter Append(Value value)
        => Append(value, out _);

    public QueryWriter Append(params Value[] values)
    {
        for (var i = 0; i != values.Length; i++)
        {
            OnNodeAdd(ref _tokens.AddLast(values[i]));
        }

        UpdateMarkers(_tokens.Count - 1, values.Length);

        return this;
    }

    public QueryWriter Append(out ValueNode startNode, params Value[] values)
    {
        if (values.Length == 0)
        {
            startNode = _tokens.Last;
            return this;
        }

        startNode = _tokens.AddLast(values[0]);

        OnNodeAdd(ref startNode);

        for (var i = 1; i < values.Length; i++)
            OnNodeAdd(ref _tokens.AddLast(values[i]));

        UpdateMarkers(_tokens.Count - 1, values.Length);

        return this;
    }

    public QueryWriter AppendIf(Func<bool> condition, Value value)
    {
        return condition() ? Append(value) : this;
    }

    public bool AppendIsEmpty(Value value)
        => AppendIsEmpty(value, out _, out _);

    public bool AppendIsEmpty(Value value, out int size)
        => AppendIsEmpty(value, out size, out _);

    public bool AppendIsEmpty(Value value, out int size, out ValueNode node)
    {
        var index = TailIndex;
        Append(value, out node);
        size = TailIndex - index;
        return size > 0;
    }

    public StringBuilder Compile(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        ref var current = ref _tokens.First;
        for (var i = 0; !Unsafe.IsNullRef(ref current); i++)
        {
            current.Value.WriteTo(this, builder, ref current, i);
            current = ref current.Next;
        }

        return builder;
    }

    public void Dispose()
    {
        _markers.Clear();
        _tokens.Clear();
        _observers.Clear();
        _markersRef.Clear();
    }
}
