using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct SetTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ushort TypePos;

        public SetTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;
            TypePos = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
