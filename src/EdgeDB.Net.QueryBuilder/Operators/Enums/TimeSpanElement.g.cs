namespace EdgeDB
{
    [EnumSerializerAttribute(SerializationMethod.Lower)]
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
