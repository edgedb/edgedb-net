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
            var id = reader.ReadGuid();

            ITypeDescriptor? descriptor = type switch
            {
                DescriptorType.ArrayTypeDescriptor => new ArrayTypeDescriptor(id, ref reader),
                DescriptorType.BaseScalarTypeDescriptor => new BaseScalarTypeDescriptor(id),
                DescriptorType.EnumerationTypeDescriptor => new EnumerationTypeDescriptor(id, ref reader),
                DescriptorType.NamedTupleDescriptor => new NamedTupleTypeDescriptor(id, ref reader),
                DescriptorType.ObjectShapeDescriptor => new ObjectShapeDescriptor(id, ref reader),
                DescriptorType.ScalarTypeDescriptor => new ScalarTypeDescriptor(id, ref reader),
                DescriptorType.ScalarTypeNameAnnotation => new ScalarTypeNameAnnotation(id, ref reader),
                DescriptorType.SetDescriptor => new SetDescriptor(id, ref reader),
                DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor(id, ref reader),
                DescriptorType.InputShapeDescriptor => new InputShapeDescriptor(id, ref reader),
                DescriptorType.RangeTypeDescriptor => new RangeTypeDescriptor(id, ref reader),
                _ => null
            };

            if(descriptor is null)
            {
                var rawType = (byte)type;

                if (rawType >= 0x80 && rawType <= 0xfe)
                {
                    descriptor = new TypeAnnotationDescriptor(type, id, reader);
                }
                else
                    throw new InvalidDataException($"No descriptor found for type {type}");
            }

            return descriptor;
        }
    }
}
