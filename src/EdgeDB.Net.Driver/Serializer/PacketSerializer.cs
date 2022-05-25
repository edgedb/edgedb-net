using EdgeDB.Codecs;
using EdgeDB.Models;
using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;

namespace EdgeDB
{
    internal class PacketSerializer
    {
        public static readonly Guid NullCodec = Guid.Empty;

        private static readonly ConcurrentDictionary<ServerMessageType, Func<PacketReader, uint, IReceiveable>> _receiveablePayloadFactory = new();
        private static readonly ConcurrentDictionary<Guid, ICodec> _codecCache = new();

        static PacketSerializer()
        {
            _receiveablePayloadFactory.TryAdd(ServerMessageType.Authentication, (r, _) => new AuthenticationStatus(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.CommandComplete, (r, _) => new CommandComplete(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.CommandDataDescription, (r, _) => new CommandDataDescription(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.Data, (r, _) => new Data(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.DumpBlock, (r, l) => new DumpBlock(r, l));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.DumpHeader, (r, l) => new DumpHeader(r, l));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ErrorResponse, (r, _) => new ErrorResponse(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.LogMessage, (r, _) => new LogMessage(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ParameterStatus, (r, _) => new ParameterStatus(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ParseComplete, (r, _) => new ParseComplete(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ReadyForCommand, (r, _) => new ReadyForCommand(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.RestoreReady, (r, _) => new RestoreReady(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ServerHandshake, (r, _) => new ServerHandshake(r));
            _receiveablePayloadFactory.TryAdd(ServerMessageType.ServerKeyData, (r, _) => new ServerKeyData(r));
        }

        public static string? GetEdgeQLType(Type t)
        {
            if (t.Name is not "Nullable`1")
                t = t.GenericTypeArguments[0];
            return _scalarTypeMap.TryGetValue(t, out var result) ? result : null;
        }

        public static Type? GetDotnetType(string? t)
        {
            var val = _scalarTypeMap.FirstOrDefault(x => x.Value == t);

            return val.Key;
        }

        public static IReceiveable? DeserializePacket(ServerMessageType type, Stream stream, EdgeDBBinaryClient client)
        {
            using(var reader = new PacketReader(stream))
            {
                return DeserializePacket(type, reader, (uint)stream.Length, client);
            }
        }
        public static IReceiveable? DeserializePacket(ServerMessageType type, byte[] buffer, EdgeDBBinaryClient client)
        {
            using (var reader = new PacketReader(buffer))
            {
                return DeserializePacket(type, reader, (uint)buffer.Length, client);
            }
        }

        public static IReceiveable? DeserializePacket(ServerMessageType type, PacketReader reader, uint length, EdgeDBBinaryClient client)
        {
            if(_receiveablePayloadFactory.TryGetValue(type, out var factory))
            {
                return factory.Invoke(reader, length);
            }
            else
            {
                // skip the packet length
                reader.Read(new byte[length], 0, (int)length);

                client.Logger.UnknownPacket(type.ToString("X"));
                return null;
            }
        }

        public static ICodec? GetCodec(Guid id)
            => _codecCache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(id);

        public static ICodec? BuildCodec(Guid id, PacketReader reader)
        {
            if (id == NullCodec)
                return new NullCodec();

            List<ICodec> codecs = new();

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var typeDescriptor = ITypeDescriptor.GetDescriptor(reader);

                var codec = GetScalarCodec(typeDescriptor.Id);

                if (codec is not null)
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
                                var codecType = typeof(Array<>).MakeGenericType(innerCodec.ConverterType);
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

                        default:
                            break;
                    }
                }
            }

            _codecCache[id] = codecs.Last();

            return codecs.Last();
        }

        public static ICodec? GetScalarCodec(Guid typeId)
        {
            if (_defaultCodecs.TryGetValue(typeId, out var codec))
            {
                // construct the codec
                var builtCodec = (ICodec)Activator.CreateInstance(codec)!;
                _codecCache[typeId] = builtCodec;
                return builtCodec;
            }

            return null;
        }

        private static readonly Dictionary<Guid, Type> _defaultCodecs = new()
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

        private static readonly Dictionary<Type, string> _scalarTypeMap = new()
        {
            { typeof(string), "str" },
            { typeof(IEnumerable<char>), "str" },
            { typeof(bool), "bool" },
            { typeof(short), "int16" },
            { typeof(ushort), "int16" },
            { typeof(int), "int32" },
            { typeof(uint), "int32" },
            { typeof(long), "int64" },
            { typeof(ulong), "int64" },
            { typeof(float), "float32" },
            { typeof(double), "float64" },
            { typeof(BigInteger), "bigint" },
            { typeof(decimal), "decimal" },
            { typeof(DataTypes.Json), "json" },
            { typeof(Guid), "uuid" },
            { typeof(byte[]), "bytes" },
            { typeof(DateTime), "local_datetime" },
            { typeof(DateTimeOffset), "datetime" },
            { typeof(TimeSpan), "duration" },
            { typeof(Sequence), "sequence" }
        };
    }
}
