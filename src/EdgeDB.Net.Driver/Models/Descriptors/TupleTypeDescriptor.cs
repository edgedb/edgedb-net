using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct TupleTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;

        public bool IsEmpty 
            => Id.ToString() == "00000000-0000-0000-0000-0000000000FF";

        public readonly ushort[] ElementTypeDescriptorsIndex;

        public TupleTypeDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            var count = reader.ReadUInt16();

            ushort[] elements = new ushort[count];

            for (int i = 0; i != count; i++)
            {
                elements[i] = reader.ReadUInt16();
            }

            ElementTypeDescriptorsIndex = elements;
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
