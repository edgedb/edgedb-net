using EdgeDB.Compiled;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using TokenNode = EdgeDB.LooseLinkedList<EdgeDB.Token>.Node;
using TokenNodeSlice = EdgeDB.LooseLinkedList<EdgeDB.Token>.NodeSlice;

namespace EdgeDB;

internal sealed class QueryWriter(bool isDebugQuery = false) : IDisposable
{
    private readonly List<INodeObserver> _observers = [];

    private readonly Queue<int> _termUpdates = [];

    public readonly TermCollection Terms = new();

    public readonly LooseLinkedList<Token> Tokens = new();

    private TokenNode? _track;

    public bool IsDebug { get; } = isDebugQuery;

    public int TailIndex => Tokens.Count - 1;

    private int TrackedPosition { get; set; }

    public void Dispose()
    {
        Terms.Clear();
        Tokens.Clear();
        _observers.Clear();
    }

    private void UpdateTerms()
        => Terms.Update(_termUpdates, Tokens.Count);

    /// <summary>
    ///     Creates a new scope that appends the next tokens at the start of this writer until the
    ///     <see cref="IDisposable" /> is disposed.
    /// </summary>
    /// <returns>A <see cref="IDisposable" /> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScopeFromStart()
        => PositionalScope(null);

    /// <summary>
    ///     Creates a new scope that appends the next tokens after the provided node until the
    ///     <see cref="IDisposable" /> is disposed.
    /// </summary>
    /// <param name="from">The node to append tokens after.</param>
    /// <returns>A <see cref="IDisposable" /> that represents the lifetime of the scope.</returns>
    public IDisposable PositionalScope(TokenNode? from)
        => new PositionalTrack(this, from);

    private TokenNodeSlice AddAfterTracked(in Token token)
        => AddTracked(in token, true);

    private TokenNodeSlice AddBeforeTracked(in Token token)
        => AddTracked(in token, false);

    private TokenNodeSlice AddTracked(in Token token, bool after)
    {
        if (token.TryProxy(this, out var head, out var tail))
        {
            // track is already updated.
            return TokenNodeSlice.Create(head, tail);
        }

        if (_track is null)
        {
            _track = Tokens.AddFirst(in token);
            _termUpdates.Enqueue(TrackedPosition);
        }
        else
        {
            _track = after
                ? Tokens.AddAfter(_track, in token)
                : Tokens.AddBefore(_track, in token);

            _termUpdates.Enqueue(TrackedPosition - (after ? 0 : 1));
        }

        OnNodeAdd(_track);

        TrackedPosition++;

        return TokenNodeSlice.Create(_track, _track);
    }

    public void AddObserver(INodeObserver observer)
        => _observers.Add(observer);

    public void RemoveObserver(INodeObserver observer)
        => _observers.Remove(observer);

    private void OnNodeAdd(TokenNode node)
    {
        foreach (var observer in _observers)
            observer.OnAdd(node);
    }

    private void OnNodeRemove(TokenNode node)
    {
        foreach (var observer in _observers)
            observer.OnRemove(node);
    }

    private int GetIndexOfNode(TokenNode node)
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

    public QueryWriter Term(TermType type, string name, in Token token, Deferrable<string>? debug1 = null,
        ITermMetadata? metadata = null)
    {
        if (type is TermType.Verbose && !IsDebug)
        {
            Append(in token);
            return this;
        }

        var sizeDelta = Tokens.Count;
        var position = TrackedPosition;

        Append(in token, out var slice);

        var size = Tokens.Count - sizeDelta;

        Terms.Add(new Term(name, type, this, size, position, slice, debug1, metadata));
        return this;
    }

    public QueryWriter Term(TermType type, string name, Deferrable<string>? debug = null,
        ITermMetadata? metadata = null)
        => Term(type, name, debug, metadata, name);

    public QueryWriter Term(TermType type, string name, Deferrable<string>? debug = null, params Token[] tokens)
        => Term(type, name, debug, null, tokens);

    public QueryWriter Term(TermType type, string name, Deferrable<string>? debug = null,
        ITermMetadata? metadata = null, params Token[] tokens)
    {
        if (type is TermType.Verbose && !IsDebug)
        {
            Append(tokens);
            return this;
        }

        if (tokens.Length == 0)
            return this;

        var sizeDelta = Tokens.Count;
        var position = TrackedPosition;

        Append(out var slice, tokens);

        var size = Tokens.Count - sizeDelta;

        Terms.Add(new Term(
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

    public QueryWriter Remove(int position, TokenNodeSlice slice)
    {
        var totalRemoved = Tokens.RemoveSlice(slice);
        Terms.Remove(position..totalRemoved);
        return this;
    }

    public QueryWriter Remove(int position, TokenNode head, int count = 1)
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

        Terms.Remove(position..count);

        return this;
    }

    public QueryWriter Move(TokenNodeSlice target, Range targetRange, TokenNodeSlice value, Range valueRange)
    {
        // move value to the new location
        Tokens.MoveSlice(value, target.Head);

        // remove the old value
        Tokens.RemoveSlice(target);

        // update terms to reflect the delete
        Terms.Remove(targetRange);

        // update terms to reflect the move
        Terms.Move(valueRange, targetRange);

        return this;
    }

    public QueryWriter Move(int position, TokenNodeSlice target, in Token token)
    {
        var oldTrack = _track;

        _track = target.Tail;
        AddAfterTracked(in token);
        _track = oldTrack;

        Remove(position, target);
        return this;
    }

    public QueryWriter Strip(TokenNodeSlice slice, Range sliceRange, TokenNodeSlice keep, Range keepRange)
    {
        Tokens.Strip(slice, keep, out var sliceSize, out var keepSize, out var keepOffset);

        if (keepOffset > 0)
        {
            Terms.Remove(sliceRange.Start..keepOffset);
        }

        if (keepOffset + keepSize < sliceSize)
        {
            Terms.Remove((sliceRange.Start.Value + keepOffset + keepSize)..(sliceSize - keepOffset - keepSize));
        }

        return this;
    }

    public QueryWriter Append(in Token token, out TokenNodeSlice node)
    {
        node = AddAfterTracked(in token);

        UpdateTerms();

        return this;
    }

    public QueryWriter Append(in Token token, out TokenNodeSlice node, out int size)
    {
        var sizeDelta = Tokens.Count;

        Append(in token, out node);

        size = Tokens.Count - sizeDelta;

        return this;
    }

    public QueryWriter Append(in Token token)
        => Append(in token, out _);

    public QueryWriter Append(params Token[] tokens)
    {
        for (var i = 0; i != tokens.Length; i++)
        {
            AddAfterTracked(in tokens[i]);
        }

        UpdateTerms();

        return this;
    }

    public QueryWriter Append(out TokenNodeSlice node, params Token[] tokens)
    {
        if (tokens.Length == 0)
        {
            throw new ArgumentException("Values must contain at least 1 value");
        }

        node = AddAfterTracked(in tokens[0]);

        for (var i = 1; i < tokens.Length; i++)
            node.Tail = AddAfterTracked(in tokens[i]).Tail ?? node.Tail;

        UpdateTerms();

        return this;
    }

    public QueryWriter AppendIf(Func<bool> condition, in Token token) => condition() ? Append(in token) : this;

    public bool AppendIsEmpty(in Token token)
        => AppendIsEmpty(in token, out _, out _);

    public bool AppendIsEmpty(in Token token, out int size)
        => AppendIsEmpty(in token, out size, out _);

    public bool AppendIsEmpty(in Token token, out int size, [MaybeNullWhen(true)] out TokenNodeSlice node)
    {
        if (token == Token.Empty)
        {
            size = 0;
            node = new TokenNodeSlice(_track, _track);
            return true;
        }

        var index = TailIndex;
        Append(in token, out node);
        size = TailIndex - index;
        return size == 0;
    }

    public QueryWriter AppendPrefixIfNotEmpty(in Token prefix, in Token token, out bool wasEmpty)
    {
        var pos = TailIndex;
        Append(in prefix, out var prefixSlice);

        if (AppendIsEmpty(in token))
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

    public (string Query, LinkedList<QuerySpan> Terms, LinkedList<int> Tokens) CompileDebug()
    {
        var query = new StringBuilder();
        var activeTerms = new List<ActiveTermTrack>();
        var spans = new LinkedList<QuerySpan>();
        var tokens = new LinkedList<int>();
        var terms = new HashSet<(string, Term)>(Terms.TermsByName.SelectMany(x => x.Value.Select(y => (x.Key, y))));

        var current = Tokens.First;

        while (current is not null)
        {
            foreach (var activeTerm in activeTerms.ToArray())
            {
                if (activeTerm.TryWrite(current.Value)) continue;

                activeTerms.Remove(activeTerm);
                var content = activeTerm.Builder.ToString();
                spans.AddLast(new QuerySpan(activeTerm.Index..(activeTerm.Index + content.Length), content,
                    activeTerm.Term, activeTerm.Name));
            }

            foreach (var startingTerm in terms.Where(x => x.Item2.IsAlive && x.Item2.Slice.Head == current))
            {
                terms.Remove(startingTerm);
                var sb = new StringBuilder();

                activeTerms.Add(new ActiveTermTrack(query.Length, startingTerm.Item2, startingTerm.Item1, sb,
                    startingTerm.Item2.Size - 1));
                current.Value.WriteTo(sb);
            }

            var delta = query.Length;
            current.Value.WriteTo(query);
            tokens.AddLast(query.Length - delta);

            current = current.Next;
        }

        foreach (var remaining in activeTerms)
        {
            var content = remaining.Builder.ToString();
            spans.AddLast(new QuerySpan(remaining.Index..(remaining.Index + content.Length), content, remaining.Term,
                remaining.Name));
        }

        return (query.ToString(), spans, tokens);
    }

#if DEBUG
    public string QuickDebugView() => DebugCompiledQuery.QuickView(this);
#endif
    private sealed class PositionalTrack : IDisposable
    {
        private readonly TokenNode? _oldRef;
        private readonly QueryWriter _writer;

        public PositionalTrack(QueryWriter writer, TokenNode? from)
        {
            _oldRef = writer._track;

            writer.TrackedPosition = from is not null
                ? writer.GetIndexOfNode(from)
                : 0;
            writer._track = from;

            _writer = writer;
        }

        public void Dispose() => _writer._track = _oldRef;
    }

    private sealed class ActiveTermTrack(int index, Term term, string name, StringBuilder builder, int count)
    {
        public string Name { get; } = name;
        public int Index { get; } = index;
        public Term Term { get; } = term;
        public StringBuilder Builder { get; } = builder;

        public bool TryWrite(Token token)
        {
            if (count == 0)
                return false;

            token.WriteTo(Builder);
            count--;
            return true;
        }
    }
}
