namespace EdgeDB
{
    [EnumSerializerAttribute(SerializationMethod.Lower)]
    public enum DurationTruncateUnit
    {
        Microseconds,
        Milliseconds,
        Seconds,
        Minutes,
        Hours
    }
}
