namespace EdgeDB
{
    [EnumSerializer(SerializationMethod.Lower)]
    public enum DateTimeTruncateUnit
    {
        Microseconds,
        Milliseconds,
        seconds,
        minutes,
        hours,
        days,
        weeks,
        months,
        quarters,
        years,
        decades,
        centuries
    }
}
