using Gel.Binary.Protocol.Common.Descriptors;

namespace Gel.Binary.Codecs;

internal abstract class BaseScalarCodec<T>
    : BaseCodec<T>, IScalarCodec<T>
{
    protected BaseScalarCodec(in Guid id, CodecMetadata? metadata)
        : base(in id, metadata)
    {
    }
}
