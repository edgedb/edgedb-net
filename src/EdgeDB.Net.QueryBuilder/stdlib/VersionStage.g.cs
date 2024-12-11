namespace EdgeDB
{
    [EdgeDBType(ModuleName = "sys")]
    public enum VersionStage
    {
        dev,
        alpha,
        beta,
        rc,
        final,
    }
}
