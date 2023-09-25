using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.Common.Descriptors
{
    internal sealed class CodecMetadata
    {
        public string? SchemaName { get; }

        public bool IsSchemaDefined { get; }

        public CodecAncestor[] Ancestors { get; }

        public CodecMetadata(string? schemaName, bool isSchemaDefined, CodecAncestor[]? ancestors = null)
        {
            SchemaName = schemaName;
            IsSchemaDefined = isSchemaDefined;
            Ancestors = ancestors ?? Array.Empty<CodecAncestor>();
        }
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
}
