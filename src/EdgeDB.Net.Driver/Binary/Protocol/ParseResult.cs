namespace EdgeDB.Binary.Protocol;

internal sealed class ParseResult
{
    public ParseResult(CodecInfo inCodecInfo, CodecInfo outCodecInfo, scoped in ReadOnlyMemory<byte>? stateData)
    {
        InCodecInfo = inCodecInfo;
        OutCodecInfo = outCodecInfo;
        StateData = stateData;
    }

    public CodecInfo InCodecInfo { get; }
    public CodecInfo OutCodecInfo { get; }
    public ReadOnlyMemory<byte>? StateData { get; }
}
