using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary
{
    internal interface ITypeDescriptor
    {
        Guid Id { get; }

        public static ITypeDescriptor GetDescriptor(ref PacketReader reader)
        {
            var type = (DescriptorType)reader.ReadByte();

            ITypeDescriptor? descriptor = type switch
            {
                DescriptorType.ArrayTypeDescriptor => new ArrayTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.BaseScalarTypeDescriptor => new BaseScalarTypeDescriptor(reader.ReadGuid()),
                DescriptorType.EnumerationTypeDescriptor => new EnumerationTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.NamedTupleDescriptor => new NamedTupleTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.ObjectShapeDescriptor => new ObjectShapeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.ScalarTypeDescriptor => new ScalarTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.SetDescriptor => new SetTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.InputShapeDescriptor => new InputShapeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.RangeTypeDescriptor => new RangeTypeDescriptor(reader.ReadGuid(), ref reader),
                DescriptorType.TypeIntrospectionDescriptor or
                DescriptorType.DetailedTypeIntrospectionDescriptor => new TypeIntrospectionDescriptor(type, ref reader),
                DescriptorType.ScalarTypeNameAnnotation or
                DescriptorType.ScalarDetailedAnnotation => new ScalarTypeNameAnnotation(type, reader.ReadGuid(), ref reader),
                _ => null
            };

            if(descriptor is null)
            {
                var rawType = (byte)type;

                if (rawType >= 0x80 && rawType <= 0xfe)
                {
                    descriptor = new TypeAnnotationDescriptor(type, reader.ReadGuid(), ref reader);
                }
                else
                    throw new InvalidDataException($"No descriptor found for type {type}");
            }

            return descriptor;
        }
    }
}
