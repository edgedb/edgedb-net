using EdgeDB.Codecs;
using EdgeDB.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class PacketSerializer
    {
        public static readonly Guid NullCodec = Guid.Empty;

        private static Dictionary<ServerMessageType, IReceiveable> _receiveablePayload = new();
        private static Dictionary<Guid, ICodec> _codecCache = new();

        static PacketSerializer()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetTypeInfo().ImplementedInterfaces.Any(y => y == typeof(IReceiveable)));

            foreach(var t in types)
            {
                var inst = (IReceiveable)Activator.CreateInstance(t)!;
                _receiveablePayload.Add(inst.Type, inst);
            }
        }

        public static string? GetEdgeQLType(Type t)
        {
            if (t.Name == "Nullable`1")
                t = t.GenericTypeArguments[0];
            if (_scalarTypeMap.TryGetValue(t, out var result))
                return result;
            return null;
        }

        public static Type? GetDotnetType(string? t)
        {
            var val = _scalarTypeMap.FirstOrDefault(x => x.Value == t);

            return val.Key;
        }

        public static IReceiveable? DeserializePacket(ServerMessageType type, Stream stream, EdgeDBTcpClient client)
        {
            // read the type

            var reader = new PacketReader(stream);
            var length = reader.ReadUInt32() - 4;

            if (_receiveablePayload.ContainsKey(type))
            {
                var converter = _receiveablePayload[type];

                converter.Read(reader, (uint)length, client);

                return converter;
            }
            else
            {
                // skip the packet length

                stream.Read(new byte[length], 0, (int)length);

                client.Logger.LogWarning("No converter found for message type 0x{} ({})", $"{type:X}", type);
                return null;
            }
        }

        public static ICodec? GetCodec(Guid id)
        {
            if (_codecCache.TryGetValue(id, out var codec))
                return codec;

            return GetScalarCodec(id);
        }

        public static ICodec? BuildCodec(Guid id, PacketReader reader)
        {
            if (id == NullCodec)
                return new NullCodec();

            List<ICodec> codecs = new();

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var typeDescriptor = ITypeDescriptor.GetDescriptor(reader);

                var codec = GetScalarCodec(typeDescriptor.Id);

                if (codec != null)
                    codecs.Add(codec);
                else
                {
                    // create codec based on type descriptor
                    switch (typeDescriptor)
                    {
                        case EnumerationTypeDescriptor enumeration:
                            {
                                // decode as string like
                                codecs.Add(new Text());
                            }
                            break;
                        case ObjectShapeDescriptor shapeDescriptor:
                            {
                                var codecArguments = shapeDescriptor.Shapes.Select(x => (x.Name, codecs[x.TypePos]));
                                codec = new Codecs.Object(codecArguments.Select(x => x.Item2).ToArray(), codecArguments.Select(x => x.Name).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case TupleTypeDescriptor tuple:
                            {
                                codec = new Codecs.Tuple(tuple.ElementTypeDescriptorsIndex.Select(x => codecs[x]).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case NamedTupleTypeDescriptor namedTuple:
                            {
                                // TODO: better datatype than an object?
                                var codecArguments = namedTuple.Elements.Select(x => (x.Name, codecs[x.TypePos]));
                                codec = new Codecs.Object(codecArguments.Select(x => x.Item2).ToArray(), codecArguments.Select(x => x.Name).ToArray());
                                codecs.Add(codec);
                            }
                            break;
                        case ArrayTypeDescriptor array:
                            {
                                var innerCodec = codecs[array.TypePos];

                                // create the array codec with reflection
                                var codecType = typeof(Codecs.Array<>).MakeGenericType(innerCodec.ConverterType);
                                codec = (ICodec)Activator.CreateInstance(codecType, innerCodec)!;
                                codecs.Add(codec);
                            }
                            break;
                        case SetDescriptor set:
                            {
                                var innerCodec = codecs[set.TypePos];

                                var codecType = typeof(Set<>).MakeGenericType(innerCodec.ConverterType);
                                codec = (ICodec)Activator.CreateInstance(codecType, innerCodec)!;
                                codecs.Add(codec);
                            }
                            break;

                    }
                }
            }

            if(codecs.Count == 0)
            {

            }

            _codecCache.Add(id, codecs.Last());

            return codecs.Last();
        }

        public static ICodec? GetScalarCodec(Guid typeId)
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
            { NullCodec, typeof(NullCodec) },
            { new Guid("00000000-0000-0000-0000-000000000100"), typeof(UUID) },
            { new Guid("00000000-0000-0000-0000-000000000101"), typeof(Text) },
            { new Guid("00000000-0000-0000-0000-000000000102"), typeof(Bytes) },
            { new Guid("00000000-0000-0000-0000-000000000103"), typeof(Integer16) },
            { new Guid("00000000-0000-0000-0000-000000000104"), typeof(Integer32) },
            { new Guid("00000000-0000-0000-0000-000000000105"), typeof(Integer64) },
            { new Guid("00000000-0000-0000-0000-000000000106"), typeof(Float32) },
            { new Guid("00000000-0000-0000-0000-000000000107"), typeof(Float64) },
            { new Guid("00000000-0000-0000-0000-000000000108"), typeof(Codecs.Decimal) },
            { new Guid("00000000-0000-0000-0000-000000000109"), typeof(Bool) },
            { new Guid("00000000-0000-0000-0000-00000000010A"), typeof(Datetime) },
            { new Guid("00000000-0000-0000-0000-00000000010B"), typeof(LocalDateTime) },
            { new Guid("00000000-0000-0000-0000-00000000010C"), typeof(LocalDate) },
            { new Guid("00000000-0000-0000-0000-00000000010D"), typeof(LocalTime) },
            { new Guid("00000000-0000-0000-0000-00000000010E"), typeof(Duration) },
            { new Guid("00000000-0000-0000-0000-00000000010F"), typeof(Json) },
            { new Guid("00000000-0000-0000-0000-000000000110"), typeof(BigInt) },
            { new Guid("00000000-0000-0000-0000-000000000111"), typeof(RelativeDuration) },

        };

        private static Dictionary<Type, string> _scalarTypeMap = new()
        {
            { typeof(string), "str" },
            { typeof(IEnumerable<char>), "str"},
            { typeof(bool), "bool" },
            { typeof(short), "int16" },
            { typeof(ushort), "int16" },
            { typeof(int), "int32" },
            { typeof(uint), "int32" },
            { typeof(long), "int64"},
            { typeof(ulong), "int64"},
            { typeof(float), "float32"},
            { typeof(double), "float64"},
            { typeof(BigInteger), "bigint" },
            { typeof(decimal), "decimal"},
            { typeof(DataTypes.Json), "json"},
            { typeof(Guid), "uuid"},
            { typeof(byte[]), "bytes"},
            { typeof(DateTime), "local_datetime" },
            { typeof(DateTimeOffset), "datetime"},
            { typeof(TimeSpan), "duration"},
            { typeof(Sequence), "sequence"}
        };
    }
}
