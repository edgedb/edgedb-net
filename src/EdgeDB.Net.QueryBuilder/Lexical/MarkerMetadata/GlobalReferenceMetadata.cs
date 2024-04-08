namespace EdgeDB;

internal sealed record GlobalReferenceMetadata(QueryGlobal Global, string? EdgeDBType = null) : IMarkerMetadata;
