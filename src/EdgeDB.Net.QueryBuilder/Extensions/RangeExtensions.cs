namespace EdgeDB;

public static class RangeExtensions
{
    public static bool Overlaps(this Range a, Range b) => a.Start.Value < b.End.Value && b.Start.Value < a.End.Value;

    public static Range Normalize(this Range range) => range.Start..(range.End.Value + range.Start.Value);

    public static bool Contains(this Range range, int point)
        => range.Start.Value <= point && range.End.Value >= point;
}
