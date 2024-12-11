namespace EdgeDB;

internal sealed record GlobalMetadata(QueryGlobal Global, string? EdgeDBType = null) : ITermMetadata;
