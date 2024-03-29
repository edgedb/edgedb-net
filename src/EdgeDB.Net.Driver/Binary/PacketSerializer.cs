using EdgeDB.Binary.Protocol;
using EdgeDB.DataTypes;
using System.Numerics;
using DateTime = System.DateTime;

namespace EdgeDB.Binary;

internal sealed class PacketSerializer
{
    private static readonly Dictionary<Type, string> _scalarTypeMap = new()
    {
        {typeof(string), "str"},
        {typeof(IEnumerable<char>), "str"},
        {typeof(bool), "bool"},
        {typeof(short), "int16"},
        {typeof(ushort), "int16"},
        {typeof(int), "int32"},
        {typeof(uint), "int32"},
        {typeof(long), "int64"},
        {typeof(ulong), "int64"},
        {typeof(float), "float32"},
        {typeof(double), "float64"},
        {typeof(BigInteger), "bigint"},
        {typeof(decimal), "decimal"},
        {typeof(Json), "json"},
        {typeof(Guid), "uuid"},
        {typeof(byte[]), "bytes"},
        {typeof(DateTime), "local_datetime"},
        {typeof(DateTimeOffset), "datetime"},
        {typeof(TimeSpan), "duration"},
        {typeof(DateDuration), "date_duration"},
        {typeof(DataTypes.DateTime), "datetime"},
        {typeof(Duration), "duration"},
        {typeof(LocalDate), "local_date"},
        {typeof(LocalDateTime), "local_datetime"},
        {typeof(LocalTime), "local_time"},
        {typeof(RelativeDuration), "relative_duration"}
    };

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

    public static IReceiveable DeserializePacket(in PacketReadFactory factory, in Memory<byte> buffer)
    {
        var reader = new PacketReader(buffer.Span);
        return factory(ref reader, buffer.Length);
    }
}
