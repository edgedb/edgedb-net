using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    public interface ITypeDescriptor
    {
        DescriptorType Type { get; }
        Guid Id { get; }

        void Read(PacketReader reader);

        public static ITypeDescriptor GetDescriptor(PacketReader reader)
        {
            var type = (DescriptorType)reader.ReadByte();
            var id = reader.ReadGuid();

            ITypeDescriptor? descriptor = type switch
            {
                DescriptorType.ArrayTypeDescriptor => new ArrayTypeDescriptor { Id = id },
                DescriptorType.BaseScalarTypeDescriptor => new BaseScalarTypeDescriptor { Id = id },
                DescriptorType.EnumerationTypeDescriptor => new EnumerationTypeDescriptor { Id = id },
                DescriptorType.NamedTupleDescriptor => new NamedTupleTypeDescriptor { Id = id },
                DescriptorType.ObjectShapeDescriptor => new ObjectShapeDescriptor { Id = id },
                DescriptorType.ScalarTypeDescriptor => new ScalarTypeDescriptor { Id = id },
                DescriptorType.ScalarTypeNameAnnotation => new ScalarTypeNameAnnotation { Id = id },
                DescriptorType.SetDescriptor => new SetDescriptor { Id = id },
                DescriptorType.TupleTypeDescriptor => new TupleTypeDescriptor { Id = id },
                _ => null
            };

            if(descriptor == null)
            {
                var rawType = (byte)type;

                if (rawType >= 0x80 && rawType <= 0xfe)
                {
                    descriptor = new TypeAnnotationDescriptor { Id = id, Type = type };
                }
                else
                    throw new InvalidDataException($"No descriptor found for type {type}");
            }

            descriptor.Read(reader);

            return descriptor;
        }
    }
}
