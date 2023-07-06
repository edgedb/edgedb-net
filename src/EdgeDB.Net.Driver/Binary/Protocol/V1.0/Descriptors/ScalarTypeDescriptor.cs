using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct ScalarTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly ushort BaseTypePos;

        public ScalarTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;
            BaseTypePos = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
