using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal readonly struct RangeTypeDescriptor : ITypeDescriptor
    {
        public readonly Guid Id;
        public readonly ushort TypePos;

        public RangeTypeDescriptor(Guid id, ref PacketReader reader)
        {
            Id = id;
            TypePos = reader.ReadUInt16();
        }

        Guid ITypeDescriptor.Id => Id;
    }
}
