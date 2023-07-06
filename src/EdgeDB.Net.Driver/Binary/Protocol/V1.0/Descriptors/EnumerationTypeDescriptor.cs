using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Protocol.V1._0.Descriptors
{
    internal readonly struct EnumerationTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public readonly string[] Members;

        public EnumerationTypeDescriptor(scoped in Guid id, scoped ref PacketReader reader)
        {
            Id = id;

            var count = reader.ReadUInt16();

            string[] members = new string[count];

            for (ushort i = 0; i != count; i++)
            {
                members[i] = reader.ReadString();
            }

            Members = members;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
