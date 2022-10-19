namespace EdgeDB
{
    [EdgeDBType(ModuleName = "sys")]
    public enum TransactionIsolation
    {
        RepeatableRead,
        Serializable,
    }
}
