using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct ScalarTypeNameAnnotation : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly string Name;

        public ScalarTypeNameAnnotation(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;
            Name = reader.ReadString();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
