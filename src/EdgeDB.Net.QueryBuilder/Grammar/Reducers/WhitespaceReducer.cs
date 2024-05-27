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

    private static void Trim(QueryWriter writer, int position, LooseLinkedList<Value>.Node node, bool dir)
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

    public static bool IsWhitespace(in Value value)
    {
        if (value.CharValue.HasValue)
            return char.IsWhiteSpace(value.CharValue.Value);

        return value.StringValue is not null && string.IsNullOrWhiteSpace(value.StringValue);
    }

    public static void TrimWhitespaceAround(QueryWriter writer, Marker marker)
    {
        if (marker.Slice.Head?.Previous is not null)
            Trim(writer, marker.Position - 1, marker.Slice.Head.Previous, false);
        if(marker.Slice.Tail?.Next is not null)
            Trim(writer, marker.Position + marker.Size + 1, marker.Slice.Tail.Next, true);
    }
}
