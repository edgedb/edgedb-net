namespace EdgeDB.Binary.Protocol;

internal class ExecuteResult
{
    public ExecuteResult(ReadOnlyMemory<byte>[] data, CodecInfo outCodecInfo)
    {
        Data = data;
        OutCodecInfo = outCodecInfo;
    }

    public CodecInfo OutCodecInfo { get; }
    public ReadOnlyMemory<byte>[] Data { get; }
}
