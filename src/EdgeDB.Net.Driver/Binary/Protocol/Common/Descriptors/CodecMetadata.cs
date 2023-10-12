using EdgeDB.Binary.Codecs;

namespace EdgeDB.Binary.Protocol.Common.Descriptors;

internal sealed class CodecMetadata
{
    public CodecMetadata(string? schemaName, bool isSchemaDefined, CodecAncestor[]? ancestors = null)
    {
        SchemaName = schemaName;
        IsSchemaDefined = isSchemaDefined;
        Ancestors = ancestors ?? Array.Empty<CodecAncestor>();
    }

    public string? SchemaName { get; }

    public bool IsSchemaDefined { get; }

    public CodecAncestor[] Ancestors { get; }
}

internal struct CodecAncestor
{
    public ICodec? Codec;
    public ITypeDescriptor Descriptor;

    public CodecAncestor(ref ICodec? codec, ref ITypeDescriptor descriptor)
    {
        Codec = codec;
        Descriptor = descriptor;
    }
}
