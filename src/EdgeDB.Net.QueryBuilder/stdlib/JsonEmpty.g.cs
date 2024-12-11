namespace EdgeDB
{
    [EdgeDBType(ModuleName = "std")]
    public enum JsonEmpty
    {
        ReturnEmpty,
        ReturnTarget,
        Error,
        UseNull,
        DeleteKey,
    }
}
