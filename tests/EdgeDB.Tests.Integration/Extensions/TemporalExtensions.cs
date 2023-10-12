using System;

namespace EdgeDB.Tests.Integration;

internal static class TemporalExtensions
{
    public static TimeSpan RoundToMicroseconds(this TimeSpan t) =>
        TimeSpan.FromMicroseconds(Math.Round(t.TotalMicroseconds));

    public static DateTime RoundToMicroseconds(this DateTime t)
    {
        // divide by 10 to cut off 100-nanosecond component
        var microseconds = Math.Round(t.Ticks / 10d);

        var a = t.AddMicroseconds(Math.Round(microseconds) != microseconds ? 1 : 0);

        var r = new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, a.Second, a.Millisecond, a.Microsecond);

        return r;
    }

    public static DateTimeOffset RoundToMicroseconds(this DateTimeOffset t) =>
        // divide by 10 to cut off 100-nanosecond component
        new(t.Ticks / 10, t.Offset);

    public static TimeOnly RoundToMicroseconds(this TimeOnly t) =>
        // divide by 10 to cut off 100-nanosecond component
        new(t.Ticks / 10);
}
