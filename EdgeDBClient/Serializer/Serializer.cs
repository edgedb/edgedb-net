using EdgeDB.Codecs;
using EdgeDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class Serializer
    {
        public static Dictionary<ServerMessageTypes, IReceiveable> ReceiveablePayload = new();

        static Serializer()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetTypeInfo().ImplementedInterfaces.Any(y => y == typeof(IReceiveable)));

            foreach(var t in types)
            {
                var inst = (IReceiveable)Activator.CreateInstance(t)!;
                ReceiveablePayload.Add(inst.Type, inst);
            }
        }


        public static IReceiveable? DeserializePacket(Stream stream, EdgeDBClient client)
        {
            // read the type

            var reader = new PacketReader(stream);

            var type = (ServerMessageTypes)reader.ReadSByte();
            var length = reader.ReadUInt32() - 4;

            if (ReceiveablePayload.ContainsKey(type))
            {
                var converter = ReceiveablePayload[type];

                converter.Read(reader, (uint)length, client);

                return converter;
            }
            else
            {
                // skip the packet length

                stream.Read(new byte[length], 0, (int)length);

                Console.WriteLine($"No converter found for message type 0x{type:X} ({type})");
                return null;
            }
        }

        public static object? DeserializeDescriptor(PacketReader reader)
        {
            return null;
        }

        public static ICodec? BuildCodec(PacketReader reader)
        {
            List<ICodec> codecs = new();

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var typeDescriptor = ITypeDescriptor.GetDescriptor(reader);

                var codec = GetCodec(typeDescriptor.Id);

                if (codec != null)
                    codecs.Add(codec);
                else
                {
                    // create based on type descriptor
                    switch (typeDescriptor)
                    {
                        case ObjectShapeDescriptor shapeDescriptor:
                            var codecArguments = shapeDescriptor.Shapes.Select(x => (x.Name, codecs[x.TypePos]));
                            codec = new Codecs.Object(codecArguments.Select(x => x.Item2).ToArray(), codecArguments.Select(x => x.Name).ToArray());
                            codecs.Add(codec);

                            break;
                    }
                }
            }
            return codecs.Last();
        }

        public static ICodec? GetCodec(Guid typeId)
        {
            if(_defaultCodecs.TryGetValue(typeId, out var codec))
            {
                // construct the codec

                return (ICodec)Activator.CreateInstance(codec)!;

            }

            return null;
        }

        private static Dictionary<Guid, Type> _defaultCodecs = new Dictionary<Guid, Type>
        {
            {new Guid("00000000-0000-0000-0000-000000000100"), typeof(UUID) },
            {new Guid("00000000-0000-0000-0000-000000000101"), typeof(Text) },
            {new Guid("00000000-0000-0000-0000-000000000102"), typeof(Bytes) },
            {new Guid("00000000-0000-0000-0000-000000000103"), typeof(Integer16) },
            {new Guid("00000000-0000-0000-0000-000000000104"), typeof(Integer32) },
            {new Guid("00000000-0000-0000-0000-000000000105"), typeof(Integer64) },
            {new Guid("00000000-0000-0000-0000-000000000106"), typeof(Float32) },
            {new Guid("00000000-0000-0000-0000-000000000107"), typeof(Float64) },
            {new Guid("00000000-0000-0000-0000-000000000108"), typeof(Codecs.Decimal) },
            {new Guid("00000000-0000-0000-0000-000000000109"), typeof(Bool) },
            {new Guid("00000000-0000-0000-0000-00000000010A"), typeof(Datetime) },
            {new Guid("00000000-0000-0000-0000-00000000010B"), typeof(LocalDatetime) },
            {new Guid("00000000-0000-0000-0000-00000000010C"), typeof(LocalDate) },
            {new Guid("00000000-0000-0000-0000-00000000010D"), typeof(LocalTime) },
            {new Guid("00000000-0000-0000-0000-00000000010E"), typeof(Duration) },
            {new Guid("00000000-0000-0000-0000-00000000010F"), typeof(Json) },
            {new Guid("00000000-0000-0000-0000-000000000110"), typeof(BigInt) },
            {new Guid("00000000-0000-0000-0000-000000000111"), typeof(RelativeDuration) },

        };
    }
}
