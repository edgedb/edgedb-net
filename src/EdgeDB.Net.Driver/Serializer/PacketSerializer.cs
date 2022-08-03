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
        public static string? GetEdgeQLType(Type t)
        {
            if (t.Name == "Nullable`1")
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
                    case ServerMessageType.ReadyForCommand:
                        return new ReadyForCommand(ref reader);
                    case ServerMessageType.RestoreReady:
                        return new RestoreReady(ref reader);
                    case ServerMessageType.ServerHandshake:
                        return new ServerHandshake(ref reader);
                    case ServerMessageType.ServerKeyData:
                        return new ServerKeyData(ref reader);
                    case ServerMessageType.StateDataDescription:
                        return new StateDataDescription(ref reader);
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
            { typeof(Sequence), "sequence" },
        };
    }
}
