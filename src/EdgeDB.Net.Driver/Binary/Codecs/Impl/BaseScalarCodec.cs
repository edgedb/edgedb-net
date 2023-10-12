using EdgeDB.Binary.Protocol.Common.Descriptors;

namespace EdgeDB.Binary.Codecs;

internal abstract class BaseScalarCodec<T>
    : BaseCodec<T>, IScalarCodec<T>
{
    protected BaseScalarCodec(in Guid id, CodecMetadata? metadata)
        : base(in id, metadata)
    {
    }
}
