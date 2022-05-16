using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal struct EnumerationTypeDescriptor : ITypeDescriptor
    {
        public DescriptorType Type 
            => DescriptorType.EnumerationTypeDescriptor;

        public Guid Id { get; set; }

        public string[] Members { get; set; }

        public void Read(PacketReader reader)
        {
            var count = reader.ReadUInt16();

            string[] members = new string[count];

            for (int i = 0; i != count; i++)
            {
                members[i] = reader.ReadString();
            }

            Members = members;
        }
    }
}
