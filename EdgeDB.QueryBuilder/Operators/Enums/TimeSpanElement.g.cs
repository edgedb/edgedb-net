namespace EdgeDB
{
    [EnumSerializer(SerializationMethod.Lower)]
    public enum TimeSpanElement
    {
        MidnightSeconds,
        Hour,
        Microseconds,
        Milliseconds,
        Minutes,
        Seconds
    }
}
