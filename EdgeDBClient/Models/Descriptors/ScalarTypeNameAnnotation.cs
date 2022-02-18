using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models.Descriptors
{
    public struct ScalarTypeNameAnnotation : ITypeDescriptor
    {
        public DescriptorType Type => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public void Read(PacketReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
