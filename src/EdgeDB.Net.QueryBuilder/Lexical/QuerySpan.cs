namespace EdgeDB;

internal sealed class QuerySpan(Range range, string content, Term term, string name)
{
    public Range Range { get; } = range;

    public string Content { get; } = content;

    public Term Term { get; } = term;
    public string Name { get; } = name;
}
