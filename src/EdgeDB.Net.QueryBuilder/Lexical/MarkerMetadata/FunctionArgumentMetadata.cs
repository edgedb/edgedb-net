namespace EdgeDB;

internal sealed record FunctionArgumentMetadata(uint Index, string FunctionName, string? NamedParameter)
    : ITermMetadata;
