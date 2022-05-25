using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal readonly struct EnumerationTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.EnumerationTypeDescriptor;

        public readonly Guid Id;

        public readonly string[] Members;

        public EnumerationTypeDescriptor(Guid id, PacketReader reader)
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
