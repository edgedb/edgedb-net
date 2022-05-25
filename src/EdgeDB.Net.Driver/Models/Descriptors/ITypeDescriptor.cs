using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal interface ITypeDescriptor
    {
        DescriptorType Type { get; }

        Guid Id { get; }

        public static ITypeDescriptor GetDescriptor(PacketReader reader)
        {
            var type = (DescriptorType)reader.ReadByte();
            var id = reader.ReadGuid();

            ITypeDescriptor? descriptor = type switch
            {
                DescriptorType.ArrayTypeDescriptor => new ArrayTypeDescriptor(id, reader),
                DescriptorType.BaseScalarTypeDescriptor => new BaseScalarTypeDescriptor(id),
                DescriptorType.EnumerationTypeDescriptor => new EnumerationTypeDescriptor(id, reader),
                DescriptorType.NamedTupleDescriptor => new NamedTupleTypeDescriptor(id, reader),
                DescriptorType.ObjectShapeDescriptor => new ObjectShapeDescriptor(id, reader),
                DescriptorType.ScalarTypeDescriptor => new ScalarTypeDescriptor(id, reader),
                DescriptorType.ScalarTypeNameAnnotation => new ScalarTypeNameAnnotation(id, reader),
                DescriptorType.SetDescriptor => new SetDescriptor(id, reader),
                DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor(id, reader),
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
