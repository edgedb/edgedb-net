using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ValueNode = EdgeDB.LooseLinkedList<EdgeDB.Value>.Node;
using ValueNodeSlice = EdgeDB.LooseLinkedList<EdgeDB.Value>.NodeSlice;

namespace EdgeDB;

internal sealed class QueryWriter(bool isDebugQuery = false) : IDisposable
{
    private sealed class PositionalTrack : IDisposable
    {
        private readonly ValueNode? _oldRef;
        private readonly int _oldTrackPosition;
        private readonly QueryWriter _writer;

        public PositionalTrack(QueryWriter writer, ValueNode? from)
        {
            _oldRef = writer._track;
            _oldTrackPosition = writer.TrackedPosition;

            writer.TrackedPosition = from is not null
                ? writer.GetIndexOfNode(from)
                : 0;
            writer._track = from;

            _writer = writer;
        }

        public void Dispose()
        {
            _writer._track = _oldRef;
        }
    }

    public readonly MarkerCollection Markers = new();

    public readonly LooseLinkedList<Value> Tokens = new();

    private readonly List<INodeObserver> _observers = [];

    private readonly Queue<int> _markerUpdates = [];

    private ValueNode? _track = null;

    public int TailIndex => Tokens.Count - 1;

    public int TrackedPosition { get; private set; }

    private void UpdateMarkers()
        => Markers.Update(_markerUpdates, Tokens.Count);

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

    private ValueNodeSlice AddAfterTracked(in Value value)
        => AddTracked(in value, true);
    private ValueNodeSlice AddBeforeTracked(in Value value)
        => AddTracked(in value, false);

    private ValueNodeSlice AddTracked(in Value value, bool after)
    {
        if (value.TryProxy(this, out var head, out var tail))
        {
            // track is already updated.
            return ValueNodeSlice.Create(head, tail);
        }

        if (_track is null)
        {
            _track = Tokens.AddFirst(in value);
            _markerUpdates.Enqueue(TrackedPosition);
        }
        else
        {
            _track = after
                ? Tokens.AddAfter(_track, in value)
                : Tokens.AddBefore(_track, in value);

            _markerUpdates.Enqueue(TrackedPosition - (after ? 0 : 1));
        }

        OnNodeAdd(_track);

        TrackedPosition++;

        return ValueNodeSlice.Create(_track, _track);
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
        var current = Tokens.First;
        for (var i = 0; current is not null; i++)
        {
            if (current == node)
                return i;

            current = current.Next;
        }

        return -1;
    }

    public QueryWriter Marker(MarkerType type, string name, in Value value, Deferrable<string>? debug1 = null, IMarkerMetadata? metadata = null)
    {
        if (type is MarkerType.Verbose && !isDebugQuery)
        {
            Append(in value);
            return this;
        }

        var sizeDelta = Tokens.Count;
        var position = TrackedPosition;

        Append(in value, out var slice);

        var size = Tokens.Count - sizeDelta;

        Markers.Add(new Marker(name, type, this, size, position, slice, debug1, metadata));
        return this;
    }

    public QueryWriter Marker(MarkerType type, string name, Deferrable<string>? debug = null, IMarkerMetadata? metadata = null)
        => Marker(type, name, debug, metadata, name);

    public QueryWriter Marker(MarkerType type, string name, Deferrable<string>? debug = null, params Value[] values)
        => Marker(type, name, debug, null, values);

    public QueryWriter Marker(MarkerType type, string name, Deferrable<string>? debug = null, IMarkerMetadata? metadata = null, params Value[] values)
    {
        if (type is MarkerType.Verbose && !isDebugQuery)
        {
            Append(values);
            return this;
        }

        if (values.Length == 0)
            return this;

        var sizeDelta = Tokens.Count;
        var position = TrackedPosition;

        Append(out var slice, values);

        var size = Tokens.Count - sizeDelta;

        Markers.Add(new Marker(
            name,
            type,
            this,
            size,
            position,
            slice,
            debug,
            metadata
        ));

        return this;
    }

    public QueryWriter Remove(int position, ValueNodeSlice slice)
    {
        var totalRemoved = Tokens.RemoveSlice(slice);
        Markers.Update(position, -totalRemoved);
        return this;
    }

    public QueryWriter Remove(int position, ValueNode head, int count = 1)
    {
        if (count == 0)
            return this;

        if (count < 0)
        {
            count *= -1;
            var countForCopy = count - 1;

            for (var i = 0; i < countForCopy; i++)
            {
                if (head.Previous is null)
                    count--;
                else
                    head = head.Previous;
            }
        }

        var headPrev = head.Previous;

        Tokens.Remove(head, count, node =>
        {
            if (node == _track)
                _track = headPrev;

            if (node is not null) OnNodeRemove(node);
        });

        Markers.Update(position, -count);

        return this;
    }

    public QueryWriter Replace(ValueNode node, int position, int size, in Value value)
    {
        var oldTrack = _track;

        _track = node;
        AddBeforeTracked(in value);
        _track = oldTrack;

        Remove(position, node, size);
        return this;
    }

    public QueryWriter Replace(int position, ValueNodeSlice slice, Range a, ValueNodeSlice value, Range b)
    {
        // we then want to update markers to exclude 'value' at its original position
        Markers.Update(b.Start.Value, -b.End.Value);

        // move value to the new location
        Tokens.MoveSlice(value, slice.Head);

        // remove the old value
        Tokens.RemoveSlice(slice);

        // since we want to replace the value, our updates to markers must reflect a delta
        // calculated as the size disparity of the value
        var delta = b.End.Value - a.End.Value;

        if (position > b.Start.Value)
            position -= b.End.Value;

        Markers.Update(a.Start.Value, delta);
        return this;
    }

    public QueryWriter Replace(int position, ValueNodeSlice slice, in Value value)
    {
        var oldTrack = _track;

        _track = slice.Tail;
        AddAfterTracked(in value);
        _track = oldTrack;

        Remove(position, slice);
        return this;
    }

    public QueryWriter Append(in Value value, out ValueNodeSlice node)
    {
        node = AddAfterTracked(in value);

        UpdateMarkers();

        return this;
    }

    public QueryWriter Append(in Value value, out ValueNodeSlice node, out int size)
    {
        var sizeDelta = Tokens.Count;

        Append(in value, out node);

        size = Tokens.Count - sizeDelta;

        return this;
    }

    public QueryWriter Append(in Value value)
        => Append(in value, out _);

    public QueryWriter Append(params Value[] values)
    {
        for (var i = 0; i != values.Length; i++)
        {
            AddAfterTracked(in values[i]);
        }

        UpdateMarkers();

        return this;
    }

    public QueryWriter Append(out ValueNodeSlice node, params Value[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must contain at least 1 value");
        }

        node = AddAfterTracked(in values[0]);

        for (var i = 1; i < values.Length; i++)
            node.Tail = AddAfterTracked(in values[i]).Tail ?? node.Tail;

        UpdateMarkers();

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

    public bool AppendIsEmpty(in Value value, out int size, [MaybeNullWhen(true)] out ValueNodeSlice node)
    {
        if (value == Value.Empty)
        {
            size = 0;
            node = new ValueNodeSlice(_track, _track);
            return true;
        }

        var index = TailIndex;
        Append(in value, out node);
        size = TailIndex - index;
        return size == 0;
    }

    public QueryWriter AppendPrefixIfNotEmpty(in Value prefix, in Value value, out bool wasEmpty)
    {
        var pos = TailIndex;
        Append(in prefix, out var prefixSlice);

        if (AppendIsEmpty(in value))
        {
            Remove(pos, prefixSlice);
            wasEmpty = true;
        }
        else wasEmpty = false;

        return this;
    }

    public StringBuilder Compile(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        var current = Tokens.First;

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

    public (string Query, LinkedList<QuerySpan> Markers, LinkedList<int> Tokens) CompileDebug()
    {
        var query = new StringBuilder();
        var activeMarkers = new List<ActiveMarkerTrack>();
        var spans = new LinkedList<QuerySpan>();
        var tokens = new LinkedList<int>();
        var markers = new HashSet<(string, Marker)>(Markers.MarkersByName.SelectMany(x => x.Value.Select(y => (x.Key, y))));

        var current = Tokens.First;

        while (current is not null)
        {
            foreach (var activeMarker in activeMarkers.ToArray())
            {
                if (activeMarker.TryWrite(current.Value)) continue;

                activeMarkers.Remove(activeMarker);
                var content = activeMarker.Builder.ToString();
                spans.AddLast(new QuerySpan(activeMarker.Index..(activeMarker.Index + content.Length), content, activeMarker.Marker, activeMarker.Name));
            }

            foreach (var startingMarker in markers.Where(x => x.Item2.IsAlive && x.Item2.Slice.Head == current))
            {
                markers.Remove(startingMarker);
                var sb = new StringBuilder();

                activeMarkers.Add(new (query.Length, startingMarker.Item2, startingMarker.Item1, sb, startingMarker.Item2.Size - 1));
                current.Value.WriteTo(sb);
            }

            var delta = query.Length;
            current.Value.WriteTo(query);
            tokens.AddLast(query.Length - delta);

            current = current.Next;
        }

        foreach (var remaining in activeMarkers)
        {
            var content = remaining.Builder.ToString();
            spans.AddLast(new QuerySpan(remaining.Index..(remaining.Index + content.Length), content, remaining.Marker, remaining.Name));
        }

        return (query.ToString(), spans, tokens);
    }

    public void Dispose()
    {
        Markers.Clear();
        Tokens.Clear();
        _observers.Clear();
    }
}
