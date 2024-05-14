namespace EdgeDB;

internal sealed class WhitespaceReducer : IReducer
{
    public static readonly WhitespaceReducer Instance = new();

    public void Reduce(IQueryBuilder builder, QueryWriter writer, Queue<IReducer> shouldRunAfter)
    {
        TrimStart(writer);
        TrimEnd(writer);
    }

    private void TrimEnd(QueryWriter writer)
    {
        var token = writer.Tokens.Last;

        var count = 0;
        while (token is not null && IsWhitespace(token.Value))
        {
            count++;

            if (token.Previous is null)
                break;

            token = token.Previous;
        }

        if (count > 0)
            writer.Remove(writer.Tokens.Count - count, token!, count);
    }

    private void TrimStart(QueryWriter writer)
    {
        var token = writer.Tokens.First;

        var count = 0;
        while (token is not null && IsWhitespace(token.Value))
        {
            count++;
            token = token.Next;
        }

        if (count > 0)
            writer.Remove(0, writer.Tokens.First!, count);
    }

    private bool IsWhitespace(in Value value)
    {
        if (value.CharValue.HasValue)
            return char.IsWhiteSpace(value.CharValue.Value);

        return value.StringValue is not null && string.IsNullOrWhiteSpace(value.StringValue);
    }
}
