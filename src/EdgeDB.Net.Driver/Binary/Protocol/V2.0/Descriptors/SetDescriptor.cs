using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V2._0.Descriptors
{
    internal readonly struct SetDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ushort Type;

        public SetDescriptor(ref PacketReader reader, in Guid id)
        {
            Id = id;
            Type = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}