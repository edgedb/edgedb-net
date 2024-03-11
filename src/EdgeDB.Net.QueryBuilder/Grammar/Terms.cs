namespace EdgeDB;

internal static class Terms
{
    #region Markers
    public static QueryWriter LabelVariable(this QueryWriter writer, string name, Deferrable<string>? debug = null, params Value[] values)
        => writer.Marker(MarkerType.Variable, name, debug, values);
    public static QueryWriter LabelVerbose(this QueryWriter writer, string name, Deferrable<string>? debug = null, params Value[] values)
        => writer.Marker(MarkerType.Verbose, name, debug, values);
    #endregion

    public static QueryWriter Wrapped(this QueryWriter writer, Value value, string separator = "()", bool spaced = false)
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        return spaced ?
            writer.Append(separator[0], ' ', value, ' ', separator[1]) :
            writer.Append(separator[0], value, separator[1]);
    }

    public static QueryWriter Wrapped(this QueryWriter writer, WriterProxy value, string separator = "()")
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        return writer.Append(separator[0], value, separator[1]);
    }

    public static QueryWriter WrappedValues(this QueryWriter writer, string separator = "()", params Value[] values)
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        var value = new Value[values.Length + 2];
        value[0] = separator[0];
        value[^1] = separator[1];
        values.CopyTo(value[1..^1].AsSpan());

        return writer.Append(value);
    }

    public static QueryWriter Shape(this QueryWriter writer, string name, Deferrable<string>? debug, params Value[] values)
    {
        var value = new Value[values.Length + 2];
        value[0] = "{ ";
        value[^1] = " }";

        values.CopyTo(value[1..^1].AsSpan());

        return writer.Marker(MarkerType.Shape, name, debug, value);
    }

    public static QueryWriter Shape<T>(this QueryWriter writer, string name, T[] elements,
        Action<QueryWriter, T> func, string parentheses = "{}", Deferrable<string>? debug = null)
    {
        if (parentheses.Length != 2)
            throw new ArgumentException("Parentheses must contain 2 characters", nameof(parentheses));

        return writer.Marker(MarkerType.Shape, name, new Value(
            writer =>
            {
                writer.Append(parentheses[0], ' ');

                for (var i = 0; i < elements.Length; i++)
                {
                    func(writer, elements[i]);

                    if (i + 1 < elements.Length)
                        writer.Append(", ");
                }

                writer.Append(' ', parentheses[1]);
            }),
            debug
        );
    }

    public static QueryWriter Shape<T>(this QueryWriter writer, string name, params T[] elements)
        where T : IWriteable
    {
        return writer.Marker(MarkerType.Shape, name, new Value(
            writer =>
            {
                writer.Append("{ ");

                for (var i = 0; i < elements.Length; i++)
                {
                    elements[i].Write(writer);

                    if (i + 1 != elements.Length)
                        writer.Append(", ");
                }

                writer.Append(" }");
            })
        );
    }

    public static QueryWriter Assignment(this QueryWriter writer, Value name, Value value)
        => writer.Append(name, " := ", value);

    public static QueryWriter TypeCast(this QueryWriter writer, Value type)
        => writer.Append("<", type, ">");

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
        public static implicit operator FunctionArg(WriterProxy writerProxy) => new(writerProxy);
    }

    public static QueryWriter Function(this QueryWriter writer, string name, params FunctionArg[] args)
    {
        return writer.Marker(MarkerType.Function, $"func_{name}", Value.Of(
            writer =>
            {
                writer.Append(name, '(');

                for (var i = 0; i < args.Length;)
                {
                    var arg = args[i++];
                    bool isEmpty = false;

                    writer.Marker(
                        MarkerType.FunctionArg,
                        $"func_{name}_arg_{i}",
                        null,
                        Value.Of(
                            writer =>
                            {
                                if (writer.AppendIsEmpty(arg.Value, out _, out var node))
                                {
                                    isEmpty = true;
                                    return;
                                }

                                // append the named part if its specified
                                if (arg.Named is not null)
                                    writer.Prepend(node, Value.Of(writer => writer.Append(arg.Named, " := ")));
                            }
                        )
                    );

                    if(isEmpty)
                        continue;

                    if (i != args.Length)
                        writer.Append(", ");
                }

                writer.Append(')');
            }
        ));
    }

    public static QueryWriter SingleQuoted(this QueryWriter writer, Value value)
        => writer.Append('\'', value, '\'');

    public static QueryWriter QueryArgument(this QueryWriter writer, Value type, Value name, Deferrable<string>? debug = null)
        => writer.Marker(MarkerType.Variable, $"variable_{name}", debug, '<', type, ">$", name);

    public static Value[] Span(this QueryWriter writer, WriterProxy proxy)
    {
        using var span = new ValueSpan(writer);
        proxy(writer);
        return span.ToTokens();
    }

    public static QueryWriter AppendSpanned(this QueryWriter writer, ref Value[]? span, Func<QueryWriter, Value[]> create)
    {
        if (span is null)
            span = create(writer);
        else
            writer.Append(span);

        return writer;
    }
}
