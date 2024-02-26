using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ValueNode = EdgeDB.LooseLinkedList<EdgeDB.Value>.Node;

namespace EdgeDB;

internal sealed class QueryWriter : IDisposable
{
    private sealed class PositionalTrack : IDisposable
    {
        private readonly RefBox<ValueNode> _oldRef;
        private readonly QueryWriter _writer;

        public PositionalTrack(QueryWriter writer, ref ValueNode from)
        {
            _oldRef = RefBox<ValueNode>.From(ref writer._track.Value);
            writer._track.Set(ref from);
            _writer = writer;
        }

        public void Dispose()
        {
            _writer._track.Set(ref _oldRef.Value);
        }
    }

    public IReadOnlyDictionary<string, LinkedList<Marker>> Markers
        => _markers;

    private readonly List<Marker> _markersRef;

    private readonly LooseLinkedList<Value> _tokens;
    private readonly Dictionary<string, LinkedList<Marker>> _markers;

    private readonly List<INodeObserver> _observers = [];

    private readonly RefBox<ValueNode> _track;

    private int TailIndex => _tokens.Count - 1;

    public QueryWriter()
    {
        _tokens = new();
        _markers = new();
        _markersRef = new();
        _track = RefBox<ValueNode>.Null;
    }

    /// <summary>
    ///     Creates a new scope that appends the next tokens at the start of this writer until the
    ///     <see cref="IDisposable"/> is disposed.
    /// </summary>
    /// <returns>A <see cref="IDisposable"/> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScopeFromStart()
        => PositionalScope(ref Unsafe.NullRef<ValueNode>());

    /// <summary>
    ///     Creates a new scope that appends the next tokens after the provided node until the
    ///     <see cref="IDisposable"/> is disposed.
    /// </summary>
    /// <param name="from">The node to append tokens after.</param>
    /// <returns>A <see cref="IDisposable"/> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScope(ref ValueNode from)
        => new PositionalTrack(this, ref from);

    private ref ValueNode AddAfterTracked(in Value value)
        => ref AddTracked(in value, true);
    private ref ValueNode AddBeforeTracked(in Value value)
        => ref AddTracked(in value, false);

    private ref ValueNode AddTracked(in Value value, bool after)
    {
        ref var node = ref value.Proxy(this, out var success);

        if (success)
        {
            if(!_track.Value.ReferenceEquals(ref node))
                _track.Set(ref node);
        }
        else if (_track.IsNull)
            _track.Set(ref _tokens.AddFirst(in value));
        else
        {
            ref var newTrack = ref after
                ? ref _tokens.AddAfter(ref _track.Value, in value)
                : ref _tokens.AddBefore(ref _track.Value, in value);

            _track.Set(ref newTrack);
        }

        return ref _track.Value;
    }

    public void AddObserver(INodeObserver observer)
        => _observers.Add(observer);

    public void RemoveObserver(INodeObserver observer)
        => _observers.Remove(observer);

    private void OnNodeAdd(ref ValueNode node)
    {
        foreach(var observer in _observers)
            observer.OnAdd(ref node);
    }

    private void OnNodeRemove(ref ValueNode node)
    {
        foreach (var observer in _observers)
            observer.OnRemove(ref node);
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
        {
            node = dir ? ref node.Next : ref node.Previous;
            count--;
        }
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
        foreach (var marker in _markersRef.Where(x => x.Position > position))
        {
            marker.Update(delta);
        }
    }

    public bool TryGetMarker(string name, [MaybeNullWhen(false)] out LinkedList<Marker> markers)
        => _markers.TryGetValue(name, out markers);

    public QueryWriter Marker(MarkerType type, string name, in Value value)
    {
        if (!_markers.TryGetValue(name, out var markers))
            _markers[name] = markers = new();

        var currentIndex = TailIndex;
        Append(in value, out var head);
        var size = TailIndex - currentIndex;

        var marker = new Marker(type, this, size, currentIndex + 1, head);
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

        var currentIndex = TailIndex;

        Append(out var head, values);

        var count = TailIndex - currentIndex;

        var marker = new Marker(
            type,
            this,
            count,
            currentIndex + 1,
            head
        );

        _markersRef.Add(marker);
        markers.AddLast(marker);
        return this;
    }

    public QueryWriter Remove(int position, ref ValueNode head, int count = 1)
    {
        var resetHead = false;

        ref var headPrev = ref head.Previous;

        _tokens.Remove(ref head, count, (ref ValueNode node) =>
        {
            // we don't set track here since 'head' would be captured and copied by the delegate.
            if (!resetHead && node.ReferenceEquals(ref _track.Value))
            {
                resetHead = true;
            }
        });

        if (resetHead)
        {
            _track.Set(ref headPrev);
        }

        UpdateMarkers(position, -count);

        return this;
    }

    public QueryWriter Replace(ref ValueNode node, int position, int size, in Value value)
    {
        ref var oldTrack = ref _track.Value;

        _track.Set(ref node);
        OnNodeAdd(ref AddBeforeTracked(in value));
        _track.Set(ref oldTrack);

        Remove(position, ref node, size);
        return this;
    }

    public QueryWriter Prepend(ref ValueNode node, in Value value)
    {
        var index = GetIndexOfNode(ref node);

        if (index == -1)
            throw new InvalidOperationException("Node cannot be found in collection of tokens");

        // change the track to the node
        ref var oldTrack = ref _track.Value;

        _track.Set(ref node);
        OnNodeAdd(ref AddBeforeTracked(in value));
        _track.Set(ref oldTrack);

        UpdateMarkers(index - 1, 1);

        return this;
    }

    public QueryWriter Append(in Value value, out RefBox<ValueNode> nodeRef)
    {
        ref var node = ref AddAfterTracked(in value);

        OnNodeAdd(ref node);
        UpdateMarkers(TailIndex, 1);

        nodeRef = RefBox<ValueNode>.From(ref node);
        return this;
    }

    public QueryWriter Append(in Value value)
        => Append(in value, out _);

    public QueryWriter Append(params Value[] values)
    {
        for (var i = 0; i != values.Length; i++)
        {
            OnNodeAdd(ref AddAfterTracked(in values[i]));
        }

        UpdateMarkers(_tokens.Count - 1, values.Length);

        return this;
    }

    public QueryWriter Append(out RefBox<ValueNode> startNode, params Value[] values)
    {
        if (values.Length == 0)
        {
            startNode = RefBox<ValueNode>.From(ref _track.Value);
            return this;
        }

        ref var node = ref AddAfterTracked(in values[0]);

        OnNodeAdd(ref node);

        for (var i = 1; i < values.Length; i++)
            OnNodeAdd(ref AddAfterTracked(in values[i]));

        UpdateMarkers(_tokens.Count - 1, values.Length);

        startNode = RefBox<ValueNode>.From(ref node);

        return this;
    }

    public QueryWriter AppendIf(Func<bool> condition, in Value value)
    {
        return condition() ? Append(in value) : this;
    }

    public bool AppendIsEmpty(in Value value)
        => AppendIsEmpty(in value, out _, out _);

    public bool AppendIsEmpty(in Value value, out int size)
        => AppendIsEmpty(in value, out size, out _);

    public bool AppendIsEmpty(in Value value, out int size, out RefBox<ValueNode> node)
    {
        var index = TailIndex;
        Append(in value, out node);
        size = TailIndex - index;
        return size == 0;
    }

    public StringBuilder Compile(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        ref var current = ref _tokens.First;

        while (!Unsafe.IsNullRef(ref current))
        {
            current.Value.WriteTo(builder);
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
