namespace EdgeDB;

internal sealed record CastMetadata(EdgeDBTypeUtils.EdgeDBTypeInfo? TypeInfo, string Type) : IMarkerMetadata;
