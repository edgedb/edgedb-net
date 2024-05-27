namespace EdgeDB;

public static class EdgeDBExtensions
{
    internal static SubQuery SelectSubQuery(this Guid id, Type queryType) =>
        new(writer => writer
            .Wrapped(writer => writer
                .Append("select ")
                .Append(queryType.GetEdgeDBTypeName())
                .Append(" filter .id = <uuid>")
                .SingleQuoted(id.ToString())
            )
        );
}
