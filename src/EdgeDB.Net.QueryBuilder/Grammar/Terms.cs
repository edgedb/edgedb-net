namespace EdgeDB;

internal static class Terms
{
    public static QueryWriter LabelVariable(this QueryWriter writer, string name, Deferrable<string>? debug = null,
        params Token[] values)
        => writer.Term(TermType.Variable, name, debug, values);

    public static QueryWriter LabelVerbose(this QueryWriter writer, string name, Deferrable<string>? debug = null,
        params Token[] values)
        => writer.Term(TermType.Verbose, name, debug, values);

    public static QueryWriter Wrapped(this QueryWriter writer, Token token, string separator = "()",
        bool spaced = false)
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        return spaced
            ? writer.Append(separator[0], ' ', token, ' ', separator[1])
            : writer.Append(separator[0], token, separator[1]);
    }

    public static QueryWriter Wrapped(this QueryWriter writer, WriterProxy value, string separator = "()")
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        return writer.Append(separator[0], value, separator[1]);
    }

    public static QueryWriter WrappedValues(this QueryWriter writer, string separator = "()", params Token[] values)
    {
        if (separator.Length != 2)
            throw new ArgumentOutOfRangeException(nameof(separator));

        var value = new Token[values.Length + 2];
        value[0] = separator[0];
        value[^1] = separator[1];
        values.CopyTo(value[1..^1].AsSpan());

        return writer.Append(value);
    }

    public static QueryWriter Shape(this QueryWriter writer, string name, Deferrable<string>? debug,
        params Token[] values)
    {
        var value = new Token[values.Length + 2];
        value[0] = "{ ";
        value[^1] = " }";

        values.CopyTo(value[1..^1].AsSpan());

        return writer.Term(TermType.Shape, name, debug, value);
    }

    public static QueryWriter Shape<T>(this QueryWriter writer, string name, T[] elements,
        Action<QueryWriter, T> func, string parentheses = "{}", Deferrable<string>? debug = null)
    {
        if (parentheses.Length != 2)
            throw new ArgumentException("Parentheses must contain 2 characters", nameof(parentheses));

        return writer.Term(TermType.Shape, name, new Token(
                writer =>
                {
                    writer.Append(parentheses[0], ' ');

                    for (var i = 0; i < elements.Length; i++)
                    {
                        var iLocal = i;
                        var isEmpty = writer.AppendIsEmpty(Token.Of(writer => func(writer, elements[iLocal])));

                        if (!isEmpty && i + 1 < elements.Length)
                            writer.Append(", ");
                    }

                    writer.Append(' ', parentheses[1]);
                }),
            debug
        );
    }

    public static QueryWriter Shape<T>(this QueryWriter writer, string name, params T[] elements)
        where T : IWriteable =>
        writer.Term(TermType.Shape, name, new Token(
            writer =>
            {
                writer.Append("{ ");

                for (var i = 0; i < elements.Length; i++)
                {
                    var iLocal = i;

                    var isEmpty = writer.AppendIsEmpty(Token.Of(writer => elements[iLocal].Write(writer)));

                    if (!isEmpty && i + 1 != elements.Length)
                        writer.Append(", ");
                }

                writer.Append(" }");
            })
        );

    public static QueryWriter Assignment(this QueryWriter writer, Token name, Token token)
        => writer.Append(name, " := ", token);

    public static QueryWriter TypeCast(this QueryWriter writer, Token type, CastMetadata? metadata = null)
        => writer.Term(
            TermType.Cast,
            "cast",
            Token.Of(writer => writer.Append('<', type, '>')),
            metadata: metadata
        );

    public static QueryWriter Function(this QueryWriter writer, string name, Deferrable<string>? debug,
        FunctionMetadata? metadata, params FunctionArg[] args) =>
        writer.Term(TermType.Function, $"func_{name}", debug, new FunctionMetadata(name), Token.Of(
            writer =>
            {
                writer.Append(name, '(');

                for (var i = 0; i < args.Length;)
                {
                    var commaPos = 0;
                    LooseLinkedList<Token>.NodeSlice? commaSlice = null;

                    if (i > 0)
                    {
                        commaPos = writer.TailIndex;
                        writer.Append(", ", out commaSlice);
                    }

                    var arg = args[i++];
                    var isEmpty = false;

                    writer.Term(
                        TermType.FunctionArg,
                        $"func_{name}_arg_{i}",
                        null,
                        new FunctionArgumentMetadata(checked((uint)i - 1), name, arg.Named),
                        Token.Of(
                            writer =>
                            {
                                if (arg.Named is not null)
                                {
                                    writer.AppendPrefixIfNotEmpty(
                                        Token.Of(writer => writer.Append(arg.Named, " := ")),
                                        arg.Token,
                                        out isEmpty
                                    );
                                }
                                else
                                {
                                    writer.Append(arg.Token);
                                }
                            }
                        )
                    );

                    if (!isEmpty || commaSlice is null) continue;

                    writer.Remove(commaPos, commaSlice);
                }

                writer.Append(')');
            }
        ));

    public static QueryWriter Function(this QueryWriter writer, string name, Deferrable<string>? debug,
        params FunctionArg[] args)
        => Function(writer, name, debug, new FunctionMetadata(name), args);

    public static QueryWriter Function(this QueryWriter writer, string name, params FunctionArg[] args)
        => Function(writer, name, null, new FunctionMetadata(name), args);

    public static QueryWriter SingleQuoted(this QueryWriter writer, Token token)
        => writer.Append('\'', token, '\'');

    public static QueryWriter QueryArgument(this QueryWriter writer, Token type, Token name,
        Deferrable<string>? debug = null, bool optional = false)
        => writer.Term(TermType.Variable, $"variable_{name}", debug, optional ? "<optional " : "<", type, ">$", name);

    public static Token[] Span(this QueryWriter writer, WriterProxy proxy)
    {
        using var span = new TokenSpan(writer);
        proxy(writer);
        return span.ToTokens();
    }

    public static QueryWriter AppendSpanned(this QueryWriter writer, ref Token[]? span,
        Func<QueryWriter, Token[]> create)
    {
        if (span is null)
            span = create(writer);
        else
            writer.Append(span);

        return writer;
    }

    public readonly struct FunctionArg
    {
        public readonly Token Token;
        public readonly string? Named;

        public FunctionArg(Token token, string? named = null)
        {
            Token = token;
            Named = named;
        }

        public static implicit operator FunctionArg(Token token) => new(token);
        public static implicit operator FunctionArg(string? str) => new(str);
        public static implicit operator FunctionArg(char ch) => new(ch);
        public static implicit operator FunctionArg(WriterProxy writerProxy) => new(writerProxy);
    }
}
