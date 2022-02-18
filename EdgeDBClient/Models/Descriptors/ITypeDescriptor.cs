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

        public static ITypeDescriptor? GetDescriptor(PacketReader reader)
        {
            var type = (DescriptorType)reader.ReadByte();
            var id = reader.ReadGuid();

            switch (type)
            {
                case DescriptorType.SetDescriptor:
                    {
                        var descriptor = new SetDescriptor()
                        {
                            Id = id,
                        };

                        descriptor.Read(reader);

                        return descriptor;
                    }
                default:
                    {
                        var rawType = (byte)type;

                        if (rawType >= 0x80 && rawType <= 0xfe)
                        {
                            var descriptor = new TypeAnnotationDescriptor()
                            {
                                Type = type,
                                Id = id
                            };

                            descriptor.Read(reader);

                            return descriptor;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
