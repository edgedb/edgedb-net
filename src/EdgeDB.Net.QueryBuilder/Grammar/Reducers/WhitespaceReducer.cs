namespace EdgeDB;

internal sealed class WhitespaceReducer : IReducer
{
    public static readonly WhitespaceReducer Instance = new();

    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        TrimStart(writer);
        TrimEnd(writer);
    }

    private void TrimEnd(QueryWriter writer)
    {
        if (writer.Tokens.Last is null)
            return;

        Trim(writer, writer.TailIndex, writer.Tokens.Last, false);
    }

    private void TrimStart(QueryWriter writer)
    {
        if (writer.Tokens.First is null)
            return;

        Trim(writer, 0, writer.Tokens.First, true);
    }

    private static void Trim(QueryWriter writer, int position, LooseLinkedList<Token>.Node node, bool dir)
    {
        var token = node;
        var lastValidNode = node;

        var count = 0;
        while (token is not null && IsWhitespace(token.Value))
        {
            count++;
            lastValidNode = token;
            token = dir ? token.Next : token.Previous;
        }

        writer.Remove(position, dir ? node : lastValidNode, count);
    }

    public static bool IsWhitespace(in Token token)
    {
        if (token.CharValue.HasValue)
            return char.IsWhiteSpace(token.CharValue.Value);

        return token.StringValue is not null && string.IsNullOrWhiteSpace(token.StringValue);
    }

    public static void TrimWhitespaceAround(QueryWriter writer, Term term)
    {
        if (term.Slice.Head?.Previous is not null)
            Trim(writer, term.Position - 1, term.Slice.Head.Previous, false);
        if (term.Slice.Tail?.Next is not null)
            Trim(writer, term.Position + term.Size + 1, term.Slice.Tail.Next, true);
    }
}
