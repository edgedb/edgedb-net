using EdgeDB.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EdgeDB;

internal class QueryStringWriter
{
    public delegate void Proxy(QueryStringWriter writer);

    public readonly struct Value
    {
        public static readonly Value Empty = new((object?)null);

        private readonly Proxy? _callback;
        private readonly object? _value;
        private readonly string? _str;
        private readonly char? _ch;

        public Value(string? str)
        {
            _str = str;
        }

        public Value(char ch)
        {
            _ch = ch;
        }

        public Value(Proxy? callback)
        {
            _callback = callback;
        }

        public Value(object? value)
        {
            _value = value;
        }

        public void WriteTo(QueryStringWriter writer)
        {
            if (_callback is not null)
                _callback(writer);
            else
            {
                if (_value is QueryStringWriter qsw)
                    writer.Append(qsw);
                else if(_str is not null)
                    writer.Append(_str);
                else if (_ch is not null)
                    writer.Append(_ch);
                else
                    writer.Append(_value);

            }
        }

        public void WriteAt(QueryStringWriter writer, int index)
        {
            if (_callback is not null)
                _callback(writer);
            else
            {
                if (_value is QueryStringWriter qsw)
                    writer.Insert(index, qsw);
                else if(_str is not null)
                    writer.Insert(index, _str);
                else if (_ch is not null)
                    writer.Insert(index, _ch.Value);
                else
                    writer.Insert(index, _value);

            }
        }


        public static implicit operator Value(string? value) => new(value);
        public static implicit operator Value(char value) => new(value);
        public static implicit operator Value(QueryStringWriter writer) => new(writer);
        public static implicit operator Value(Proxy callback) => new(callback);
    }

    public sealed class Marker
    {
        public int Position { get; private set; }
        public int Size { get; }

        private readonly QueryStringWriter _writer;

        internal Marker(QueryStringWriter writer, int size,  int position)
        {
            _writer = writer;
            Size = size;
            Position = position;
        }

        internal void Update(int delta)
        {
            Position += delta;
        }

        public void Replace(Value value)
        {
            _writer
                .Remove(Position, Size)
                .Insert(Position, value);
        }

        public void Replace(Proxy value)
            => Replace(new Value(value));
    }

    private sealed class PositionedQueryStringWriter : QueryStringWriter
    {
        private int _position;
        private readonly QueryStringWriter _parent;

        public PositionedQueryStringWriter(int position, QueryStringWriter parent)
        {
            _position = position;
            _parent = parent;
        }

        protected override void OnExternalWrite(int position, int length)
        {
            if (position <= _position)
                _position += length;
        }

        protected override void WriteInternal(char ch)
        {
            _parent._builder.Insert(_position++, ch);
            base.WriteInternal(ch);
        }

        protected override void WriteInternal(string str)
        {
            _parent._builder.Insert(_position, str);
            _position += str.Length;
            base.WriteInternal(str);
        }
    }

    private readonly Dictionary<string, List<Marker>> _labels;
    private readonly StringBuilder _builder;
    private readonly SortedList<int, IntrospectionChunk> _chunks;

    public bool RequiresIntrospection
        => _chunks.Count > 0;

    public int Position
        => _builder.Length;

    public char this[int index] => _builder[index];

    public QueryStringWriter() : this(new())
    { }

    private QueryStringWriter(StringBuilder stringBuilder)
    {
        _builder = stringBuilder;
        _chunks = new();
        _chunks = new();
        _labels = new();
    }

    private void UpdateLabels(int pos, int sz)
    {
        foreach (var label in _labels.Values.SelectMany(x => x).Where(x => x.Position <= pos))
        {
            label.Update(sz);
        }
    }

    protected virtual void WriteInternal(char ch)
    {
        _builder.Append(ch);
        UpdateLabels(Position - 1, 1);
    }

    protected virtual void WriteInternal(string str)
    {
        var pos = Position;
        _builder.Append(str);
        UpdateLabels(pos, str.Length);
    }

    protected virtual void OnExternalWrite(int position, int length) {}

    public QueryStringWriter Remove(int start, int count)
        => _builder.Remove(start, count);

    public Value Val(object? value)
        => new(value);

    public Value Val(Proxy writer)
        => new(writer);

    public QueryStringWriter GetPositionalWriter(int index = -1)
    {
        if (index is -1)
            index = Position;

        return new PositionedQueryStringWriter(index, this);
    }

    public QueryStringWriter Label(string name, Value value)
    {
        if (!_labels.TryGetValue(name, out var labels))
            _labels[name] = labels = new();

        var pos = Position;
        Append(value);
        labels.Add(new Marker(this, Position - pos, pos));
        return this;
    }

    public bool TryGetLabeled(string name, [NotNullWhen(true)] out List<Marker>? markers)
        => _labels.TryGetValue(name, out markers);

    public int IndexOf(string value, bool ignoreCase = false, int startIndex = 0)
    {
        int index;
        int length = value.Length;
        int maxSearchLength = (_builder.Length - length) + 1;

        if (ignoreCase)
        {
            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (Char.ToLower(_builder[i]) == Char.ToLower(value[0]))
                {
                    index = 1;
                    while ((index < length) && (Char.ToLower(_builder[i + index]) == Char.ToLower(value[index])))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        for (int i = startIndex; i < maxSearchLength; ++i)
        {
            if (_builder[i] == value[0])
            {
                index = 1;
                while ((index < length) && (_builder[i + index] == value[index]))
                    ++index;

                if (index == length)
                    return i;
            }
        }

        return -1;
    }

    public void Clear()
    {
        _builder.Clear();
        _chunks.Clear();
    }

    public QueryStringWriter Insert(int index, char c)
    {
        _builder.Insert(index, c);
        OnExternalWrite(index, 1);
        return this;
    }
    public QueryStringWriter Insert(int index, string s)
    {
        _builder.Insert(index, s);
        OnExternalWrite(index, s.Length);
        return this;
    }

    public QueryStringWriter Insert(int index, Value val)
    {
        val.WriteAt(this, index);
        return this;
    }

    public QueryStringWriter Insert(int index, object? o)
    {
        if (o is null)
            return this;

        return Insert(index, o.ToString()!);
    }

    public QueryStringWriter Insert(int index, QueryStringWriter writer)
    {
        var offset = _builder.Length;
        _builder.Insert(offset, writer._builder);
        OnExternalWrite(offset, writer._builder.Length);

        foreach (var chunk in writer._chunks)
        {
            _chunks.Add(chunk.Key + offset, chunk.Value);
        }

        return this;
    }

    public QueryStringWriter AppendIf(Func<bool> condition, Value value)
    {
        if (condition())
            value.WriteTo(this);

        return this;
    }

    public QueryStringWriter Append(char c)
    {
        WriteInternal(c);
        return this;
    }
    public QueryStringWriter Append(string? s)
    {
        if (s is null)
            return this;

        WriteInternal(s);
        return this;
    }

    public QueryStringWriter Append(object? o)
    {
        if (o is null)
            return this;

        Append(o.ToString()!);
        return this;
    }

    public QueryStringWriter Append(Proxy value)
        => Append(new Value(value));

    public QueryStringWriter Append(Value value)
    {
        value.WriteTo(this);
        return this;
    }

    public QueryStringWriter Append(QueryStringWriter writer)
    {
        var offset = _builder.Length;
        _builder.Append(writer._builder);
        OnExternalWrite(offset, writer._builder.Length);

        foreach (var chunk in writer._chunks)
        {
            _chunks.Add(chunk.Key + offset, chunk.Value);
        }

        return this;
    }

    public QueryStringWriter AppendIntrospected(Action<SchemaInfo, QueryStringWriter> func)
    {
        _chunks.Add(_builder.Length, new IntrospectionChunk(_builder.Length, func));
        return this;
    }

    public QueryStringWriter SingleQuoted(Value value)
        => Append('\'').Append(value).Append('\'');

    public QueryStringWriter Assignment(object name, Value value)
        => Append(name).Append(" := ").Append(value);

    public QueryStringWriter Assignment(object name, Proxy value)
        => Append(name).Append(" := ").Append(value);

    public QueryStringWriter QueryArgument(object? type, Value name)
        => QueryArgument(new Value(type), name);
    public QueryStringWriter QueryArgument(Value type, Value name)
        => TypeCast(type).Append('$').Append(name);

    public QueryStringWriter TypeCast(object? type)
        => Append('<').Append(type).Append('>');

    public QueryStringWriter TypeCast(Value type)
        => Append('<').Append(type).Append('>');

    public QueryStringWriter Wrapped(Proxy func, string chars = "()")
        => Wrapped(new Value(func), chars);
    public QueryStringWriter Wrapped(Value value, string chars = "()")
    {
        if (chars.Length is not 2)
            throw new ArgumentOutOfRangeException(nameof(chars), "must contain 2 characters");

        return Append(chars[0]).Append(value).Append(chars[1]);
    }

    public QueryStringWriter Shape<T>(params T[] elements)
        where T : notnull
    {
        Append('{');

        for (var i = 0; i != elements.Length;)
        {
            Append(elements[i++]);

            if (i != elements.Length)
                Append(", ");
        }

        Append('}');

        return this;
    }

    public QueryStringWriter Shape<T>(IEnumerable<T> elements, Action<QueryStringWriter, T> writer,
        string shapeChars = "{}")
    {
        if (shapeChars.Length is not 2)
            throw new ArgumentOutOfRangeException(nameof(shapeChars), "must contain 2 characters");

        Append(shapeChars[0]);

        using var enumerator = elements.GetEnumerator();

        enumerator.MoveNext();

        loop:

        // check for empty entries
        var i = Position;
        writer(this, enumerator.Current);

        // if nothing was written, continue the iteration without adding a delimiter
        if (i == Position)
        {
            enumerator.MoveNext();
            goto loop;
        }

        if (enumerator.MoveNext())
        {
            Append(", ");
            goto loop;
        }

        Append(shapeChars[1]);

        return this;
    }

    public QueryStringWriter Shape<T>(IEnumerable<T> elements, string shapeChars = "{}")
        => Shape(elements, (writer, value) => writer.Append(value), shapeChars);

    public readonly struct FunctionArg
    {
        public readonly Value Value;
        public readonly string? Named;

        public FunctionArg(Value value, string? named = null)
        {
            Value = value;
            Named = named;
        }

        public static implicit operator FunctionArg(Value value) => new(value);
        public static implicit operator FunctionArg(string? str) => new(str);
        public static implicit operator FunctionArg(char ch) => new(ch);
        public static implicit operator FunctionArg(Proxy proxy) => new(proxy);
    }

    public QueryStringWriter Function(object function, params FunctionArg[] args)
    {
        Append(function).Append('(');

        for (var i = 0; i < args.Length;)
        {
            var arg = args[i++];

            var pos = Position;

            Append(arg.Value);

            if(pos == Position)
                continue;

            // append the named part if its specified
            if (arg.Named is not null)
                GetPositionalWriter(pos)
                    .Append(arg.Named)
                    .Append(" := ");

            if (i != args.Length)
                Append(", ");
        }

        Append(')');

        return this;
    }

    public StringBuilder Compile(SchemaInfo? info = null)
    {
        if (RequiresIntrospection && info is null)
            throw new InvalidOperationException("Introspection is required to compile this query");

        int offset = 0;
        var sb = new QueryStringWriter();

        foreach (var chunk in _chunks)
        {
            chunk.Value.Compile(info!, sb);
            _builder.Insert(offset += chunk.Key, sb);
            sb.Clear();
        }

        return _builder;
    }

    private sealed class IntrospectionChunk(int index, Action<SchemaInfo, QueryStringWriter> compiler)
    {
        public int Index { get; } = index;

        public void Compile(SchemaInfo info, QueryStringWriter builder)
            => compiler(info, builder);
    }

    public static implicit operator QueryStringWriter(StringBuilder stringBuilder) => new(stringBuilder);
}
