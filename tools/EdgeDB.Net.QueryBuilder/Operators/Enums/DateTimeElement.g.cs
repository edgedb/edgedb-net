namespace EdgeDB
{
    [EnumSerializer(SerializationMethod.Lower)]
    public enum DateTimeElement
    {
        EpochSeconds,
        Century,
        Day,
        Decade,
        Dow,
        Doy,
        Hour,
        ISODow,
        ISOYear,
        Microseconds,
        Millennium,
        Milliseconds,
        Minutes,
        Month,
        Quarter,
        Seconds,
        Week,
        Year
    }
}
