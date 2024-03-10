namespace EdgeDB;

public static class RangeExtensions
{
    public static bool Overlaps(this Range a, Range b)
    {
        return a.Start.Value < b.End.Value && b.Start.Value < a.End.Value;
    }
}
