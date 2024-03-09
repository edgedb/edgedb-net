namespace EdgeDB;

internal sealed class QuerySpan(Range range, string content, Marker marker, string name)
{
    public Range Range { get; } = range;

    public string Content { get; } = content;

    public Marker Marker { get; } = marker;
    public string Name { get; } = name;
}
