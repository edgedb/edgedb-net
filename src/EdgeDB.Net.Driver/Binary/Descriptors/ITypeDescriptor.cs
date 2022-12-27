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
                DescriptorType.SetDescriptor => new SetTypeDescriptor(id, ref reader),
                DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor(id, ref reader),
                DescriptorType.InputShapeDescriptor => new InputShapeDescriptor(id, ref reader),
                DescriptorType.RangeTypeDescriptor => new RangeTypeDescriptor(id, ref reader),
                DescriptorType.ScalarAnnotationDescriptor => new ScalarAnnotationDescriptor(id, ref reader),
                DescriptorType.DetailedScalarAnnotationDescriptor => new DetailedScalarAnnotationDescriptor(id, ref reader),
                DescriptorType.TypeAnnotationDescriptor => new TypeAnnotationDescriptor(id, ref reader),
                DescriptorType.DetailedTypeAnnotationDescriptor => new DetailedTypeAnnotationDescriptor(id, ref reader),
                DescriptorType.AncestorTypeAnnotationDescriptor => new AncestorTypeAnnotationDescriptor(id, ref reader),
                _ => null
            };

            if(descriptor is null)
            {
                var rawType = (byte)type;

                if (rawType >= 0x80 && rawType <= 0xfe)
                {
                    descriptor = new UnknownAnnotationDescriptor(type, id, ref reader);
                }
                else
                    throw new InvalidDataException($"No descriptor found for type {type:X2}");
            }

            return descriptor;
        }
    }
}
