using EdgeDB.Binary;
using EdgeDB.Binary.Packets;
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
        private static readonly ConcurrentDictionary<Guid, ICodec> _codecCache = new();

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


        public static IReceiveable? DeserializePacket(ServerMessageType type, ref Memory<byte> buffer, int length, EdgeDBBinaryClient client)
        {
            var reader = new PacketReader(buffer.Span);

            try
            {
                switch (type)
                {
                    case ServerMessageType.Authentication:
                        return new AuthenticationStatus(ref reader);
                    case ServerMessageType.CommandComplete:
                        return new CommandComplete(ref reader);
                    case ServerMessageType.CommandDataDescription:
                        return new CommandDataDescription(ref reader);
                    case ServerMessageType.Data:
                        return new Data(ref reader);
                    case ServerMessageType.DumpBlock:
                        return new DumpBlock(ref reader, in length);
                    case ServerMessageType.DumpHeader:
                        return new DumpHeader(ref reader, in length);
                    case ServerMessageType.ErrorResponse:
                        return new ErrorResponse(ref reader);
                    case ServerMessageType.LogMessage:
                        return new LogMessage(ref reader);
                    case ServerMessageType.ParameterStatus:
                        return new ParameterStatus(ref reader);
                    case ServerMessageType.ParseComplete:
                        return new ParseComplete(ref reader);
                    case ServerMessageType.ReadyForCommand:
                        return new ReadyForCommand(ref reader);
                    case ServerMessageType.RestoreReady:
                        return new RestoreReady(ref reader);
                    case ServerMessageType.ServerHandshake:
                        return new ServerHandshake(ref reader);
                    case ServerMessageType.ServerKeyData:
                        return new ServerKeyData(ref reader);
                    default:
                        // skip the packet length
                        reader.Skip(length);

                        client.Logger.UnknownPacket(type.ToString("X"));
                        return null;
                }
            }
            finally
            {
                // ensure that we read the entire packet
                if (!reader.Empty)
                {
                    // log a warning
                    client.Logger.DidntReadTillEnd(type, length);
                }

                reader.Dispose();
            }

        }

        public static ICodec? GetCodec(Guid id)
            => _codecCache.TryGetValue(id, out var codec) ? codec : GetScalarCodec(id);

        public static ICodec? BuildCodec(Guid id, byte[] buff)
        {
            var reader = new PacketReader(buff.AsSpan());
            return BuildCodec(id, ref reader);
        }

        public static ICodec? BuildCodec(Guid id, ref PacketReader reader)
        {
            if (id == NullCodec)
                return new NullCodec();

            List<ICodec> codecs = new();

            while (!reader.Empty)
            {
                var typeDescriptor = ITypeDescriptor.GetDescriptor(ref reader);

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
