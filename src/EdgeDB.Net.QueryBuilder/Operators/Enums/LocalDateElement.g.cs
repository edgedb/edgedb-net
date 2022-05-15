namespace EdgeDB
{
    [EnumSerializerAttribute(SerializationMethod.Lower)]
    public enum LocalDateElement
    {
        Century,
        Day,
        Decade,
        Dow,
        Doy,
        ISODow,
        ISOYear,
        Millennium,
        Month,
        Quarter,
        Week,
        Year
    }
}
