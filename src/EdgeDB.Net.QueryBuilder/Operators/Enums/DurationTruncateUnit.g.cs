namespace EdgeDB
{
    [EnumSerializer(SerializationMethod.Lower)]
    public enum DurationTruncateUnit
    {
        Microseconds,
        Milliseconds,
        Seconds,
        Minutes,
        Hours
    }
}
