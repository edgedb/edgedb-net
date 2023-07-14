using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct TypeAnnotationTextDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ushort Descriptor;

        public readonly string Key;

        public readonly string Value;

        public TypeAnnotationTextDescriptor(ref PacketReader reader)
        {
            Id = Guid.Empty;

            Descriptor = reader.ReadUInt16();

            Key = reader.ReadString();
            Value = reader.ReadString();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
