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
        private readonly ValueNode? _oldRef;
        private readonly QueryWriter _writer;

        public PositionalTrack(QueryWriter writer, ValueNode? from)
        {
            _oldRef = writer._track;
            writer._track = from;
            _writer = writer;
        }

        public void Dispose()
        {
            _writer._track = _oldRef;
        }
    }

    public readonly Dictionary<string, LinkedList<Marker>> Markers;

    private readonly List<Marker> _markersRef;

    private readonly LooseLinkedList<Value> _tokens;

    private readonly List<INodeObserver> _observers = [];

    private ValueNode? _track;

    private int TailIndex => _tokens.Count - 1;

    public QueryWriter()
    {
        _tokens = new();
        Markers = new();
        _markersRef = new();
        _track = null;
    }

    /// <summary>
    ///     Creates a new scope that appends the next tokens at the start of this writer until the
    ///     <see cref="IDisposable"/> is disposed.
    /// </summary>
    /// <returns>A <see cref="IDisposable"/> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScopeFromStart()
        => PositionalScope(null);

    /// <summary>
    ///     Creates a new scope that appends the next tokens after the provided node until the
    ///     <see cref="IDisposable"/> is disposed.
    /// </summary>
    /// <param name="from">The node to append tokens after.</param>
    /// <returns>A <see cref="IDisposable"/> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScope(ValueNode? from)
        => new PositionalTrack(this, from);

    private ValueNode AddAfterTracked(in Value value)
        => AddTracked(in value, true);
    private ValueNode AddBeforeTracked(in Value value)
        => AddTracked(in value, false);

    private ValueNode AddTracked(in Value value, bool after)
    {
        if (value.TryProxy(this, out var head, out _))
        {
            // track is already updated.
            return head;
        }
        else if (_track is null)
            _track = _tokens.AddFirst(in value); //Set(ref _tokens.AddFirst(in value));
        else
        {
            _track = after
                ? _tokens.AddAfter(_track, in value)
                : _tokens.AddBefore(_track, in value);
        }

        return _track;
    }

    public void AddObserver(INodeObserver observer)
        => _observers.Add(observer);

    public void RemoveObserver(INodeObserver observer)
        => _observers.Remove(observer);

    private void OnNodeAdd(ValueNode node)
    {
        foreach(var observer in _observers)
            observer.OnAdd(node);
    }

    private void OnNodeRemove(ValueNode node)
    {
        foreach (var observer in _observers)
            observer.OnRemove(node);
    }

    private int GetIndexOfNode(ValueNode node)
    {
        var current = _tokens.First;
        for (var i = 0; current is not null; i++)
        {
            if (current == node)
                return i;

            current = current.Next;
        }

        return -1;
    }

    private ValueNode? Traverse(ValueNode from, int count, bool dir)
    {
        var node = from;
        while (count > 0 && node is not null)
        {
            node = dir ? node.Next : node.Previous;
            count--;
        }
        return node;
    }

    private ValueNode? FastGetNodeFromIndex(int index)
    {
        // check for start/end node
        if (index == 0)
            return _tokens.First;

        if (index == TailIndex)
            return _tokens.Last;

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
                return Traverse(marker.Start, Math.Abs(marker.Position - index), index > marker.Position);
        }

        return normalizedDistance == index
            ? Traverse(_tokens.First!, index, true)
            : Traverse(_tokens.Last!, _tokens.Count - index, false);
    }

    private void UpdateMarkers(int position, int delta)
    {
        foreach (var marker in _markersRef.Where(x => x.Position > position))
        {
            marker.Update(delta);
        }
    }

    public bool TryGetMarker(string name, [MaybeNullWhen(false)] out LinkedList<Marker> markers)
        => Markers.TryGetValue(name, out markers);

    public QueryWriter Marker(MarkerType type, string name, in Value value, Deferrable<string>? debug = null)
    {
        if (!Markers.TryGetValue(name, out var markers))
            Markers[name] = markers = new();

        var currentIndex = TailIndex;
        Append(in value, out var head);
        var size = TailIndex - currentIndex;

        var marker = new Marker(type, this, size, currentIndex + 1, head, debug);
        _markersRef.Add(marker);
        markers.AddLast(marker);
        return this;
    }

    public QueryWriter Marker(MarkerType type, string name, Deferrable<string>? debug = null)
        => Marker(type, name, debug, name);

    public QueryWriter Marker(MarkerType type, string name, Deferrable<string>? debug = null, params Value[] values)
    {
        if (values.Length == 0)
            return this;

        if (!Markers.TryGetValue(name, out var markers))
            Markers[name] = markers = new();

        var currentIndex = TailIndex;

        Append(out var head, values);

        var count = TailIndex - currentIndex;

        var marker = new Marker(
            type,
            this,
            count,
            currentIndex + 1,
            head,
            debug
        );

        _markersRef.Add(marker);
        markers.AddLast(marker);
        return this;
    }

    public QueryWriter Remove(int position, ValueNode head, int count = 1)
    {
        var headPrev = head.Previous;

        _tokens.Remove(head, count, (node) =>
        {
            if (node == _track)
                _track = headPrev;
        });

        UpdateMarkers(position, -count);

        return this;
    }

    public QueryWriter Replace(ValueNode node, int position, int size, in Value value)
    {
        var oldTrack = _track;

        _track = node;
        OnNodeAdd(AddBeforeTracked(in value));
        _track = oldTrack;

        Remove(position, node, size);
        return this;
    }

    public QueryWriter Prepend(ValueNode node, in Value value)
    {
        var index = GetIndexOfNode(node);

        if (index == -1)
            throw new InvalidOperationException("Node cannot be found in collection of tokens");

        // change the track to the node
        var oldTrack = _track;

        _track = node;
        OnNodeAdd(AddBeforeTracked(in value));
        _track = oldTrack;

        UpdateMarkers(index - 1, 1);

        return this;
    }

    public QueryWriter Append(in Value value, out ValueNode node)
    {
        node = AddAfterTracked(in value);

        OnNodeAdd(node);
        UpdateMarkers(TailIndex, 1);

        return this;
    }

    public QueryWriter Append(in Value value)
        => Append(in value, out _);

    public QueryWriter Append(params Value[] values)
    {
        for (var i = 0; i != values.Length; i++)
        {
            OnNodeAdd(AddAfterTracked(in values[i]));
        }

        UpdateMarkers(_tokens.Count - 1, values.Length);

        return this;
    }

    public QueryWriter Append(out ValueNode node, params Value[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must contain at least 1 value");
        }

        node = AddAfterTracked(in values[0]);

        OnNodeAdd(node);

        for (var i = 1; i < values.Length; i++)
            OnNodeAdd(AddAfterTracked(in values[i]));

        UpdateMarkers(_tokens.Count - 1, values.Length);

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

    public bool AppendIsEmpty(in Value value, out int size, out ValueNode node)
    {
        var index = TailIndex;
        Append(in value, out node);
        size = TailIndex - index;
        return size == 0;
    }

    public StringBuilder Compile(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        var current = _tokens.First;

        while (current is not null)
        {
            current.Value.WriteTo(builder);
            current = current.Next;
        }

        return builder;
    }

    private sealed class ActiveMarkerTrack(int index, Marker marker, string name, StringBuilder builder, int count)
    {
        public string Name { get; } = name;
        public int Index { get; } = index;
        public Marker Marker { get; } = marker;
        public StringBuilder Builder { get; } = builder;
        public bool TryWrite(Value value)
        {
            if (count == 0)
                return false;

            value.WriteTo(Builder);
            count--;
            return true;
        }
    }

    public (string Query, LinkedList<QuerySpan> Markers) CompileDebug()
    {
        var query = new StringBuilder();
        var activeMarkers = new List<ActiveMarkerTrack>();
        var spans = new LinkedList<QuerySpan>();
        var markers = new HashSet<(string, Marker)>(Markers.SelectMany(x => x.Value.Select(y => (x.Key, y))));

        var current = _tokens.First;

        while (current is not null)
        {
            foreach (var activeMarker in activeMarkers.ToArray())
            {
                if (activeMarker.TryWrite(current.Value)) continue;

                activeMarkers.Remove(activeMarker);
                var content = activeMarker.Builder.ToString();
                spans.AddLast(new QuerySpan(activeMarker.Index..(activeMarker.Index + content.Length), content, activeMarker.Marker, activeMarker.Name));
            }

            foreach (var startingMarker in markers.Where(x => x.Item2.Start == current))
            {
                markers.Remove(startingMarker);
                var sb = new StringBuilder();

                activeMarkers.Add(new (query.Length, startingMarker.Item2, startingMarker.Item1, sb, startingMarker.Item2.Size - 1));
                current.Value.WriteTo(sb);
            }

            current.Value.WriteTo(query);

            current = current.Next;
        }

        foreach (var remaining in activeMarkers)
        {
            var content = remaining.Builder.ToString();
            spans.AddLast(new QuerySpan(remaining.Index..(remaining.Index + content.Length), content, remaining.Marker, remaining.Name));
        }

        return (query.ToString(), spans);
    }

    public void Dispose()
    {
        Markers.Clear();
        _tokens.Clear();
        _observers.Clear();
        _markersRef.Clear();
    }
}
