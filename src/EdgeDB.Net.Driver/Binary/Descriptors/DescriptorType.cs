using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal enum DescriptorType : byte
    {
        SetDescriptor = 0x00,
        ObjectShapeDescriptor = 0x01,
        BaseScalarTypeDescriptor = 0x02,
        ScalarTypeDescriptor = 0x03,
        TupleTypeDescriptor = 0x04,
        NamedTupleDescriptor = 0x05,
        ArrayTypeDescriptor = 0x06,
        EnumerationTypeDescriptor = 0x07,
        InputShapeDescriptor = 0x08,
        RangeTypeDescriptor = 0x09,
        TypeIntrospectionDescriptor = 0xfe,
        ScalarTypeNameAnnotation = 0xff,
    }
}
