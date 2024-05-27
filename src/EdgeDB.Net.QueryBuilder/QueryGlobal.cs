using EdgeDB.QueryNodes;
using EdgeDB.Schema;

namespace EdgeDB;

/// <summary>
///     Represents a globally defined variables contained within a 'WITH' statement.
/// </summary>
internal class QueryGlobal
{
    /// <summary>
    ///     Constructs a new <see cref="QueryGlobal" />.
    /// </summary>
    /// <param name="name">The name of the global.</param>
    /// <param name="value">The value which will be the assignment of this global.</param>
    public QueryGlobal(string name, object? value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Constructs a new <see cref="QueryGlobal" />.
    /// </summary>
    /// <param name="name">The name of the global.</param>
    /// <param name="value">The value which will be the assignment of this global.</param>
    /// <param name="reference">The refrence object that caused this global to be created.</param>
    public QueryGlobal(string name, object? value, object? reference)
    {
        Name = name;
        Value = value;
        Reference = reference;
    }

    /// <summary>
    ///     Gets the value that was included in the 'WITH' statement.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    ///     Gets the object reference that the <see cref="Value" /> represents.
    ///     For example the following code
    ///     <code>
    ///     QueryBuilder.Insert(new Person {..., Friend = new Person {...}})
    ///     </code>
    ///     would cause the nested person object to be converted to a <see cref="QueryGlobal" />
    ///     and this property would be the actual reference to that person instance.
    /// </summary>
    public object? Reference { get; init; }

    /// <summary>
    ///     Gets the name of the global.
    /// </summary>
    public string Name { get; init; }

    public Token[] Compile(IQueryBuilder source, QueryWriter writer, CompileContext? context = null,
        SchemaInfo? info = null)
        => Compile(source, QueryBuilderExtensions.WriteTo, writer, context, info);

    public Token[] Compile(ExpressionContext source, QueryWriter writer, CompileContext? context = null,
        SchemaInfo? info = null)
        => Compile(source, QueryBuilderExtensions.WriteTo, writer, context, info);

    public Token[] Compile(QueryNode source, QueryWriter writer, CompileContext? context = null,
        SchemaInfo? info = null)
        => Compile(source, QueryBuilderExtensions.WriteTo, writer, context, info);

    private Token[] Compile<T>(T source, Action<IQueryBuilder, QueryWriter, T, CompileContext?> compileBuilder,
        QueryWriter writer, CompileContext? context = null,
        SchemaInfo? info = null) =>
        writer.Span(writer => writer
            .Term(
                TermType.GlobalDeclaration,
                Name,
                Defer.This(() =>
                    $"Reference?: {Reference?.GetType().ToString() ?? "null"}, Value?: {Value?.GetType().ToString() ?? "null"}"),
                new GlobalMetadata(this),
                Token.Of(writer =>
                {
                    switch (Value)
                    {
                        case IQueryBuilder queryBuilder:
                            writer.Term(
                                TermType.SubQuery,
                                "sub_query_from_query_builder",
                                Token.Of(writer => writer
                                    .Wrapped(writer =>
                                        compileBuilder(queryBuilder, writer, source,
                                            context ?? new CompileContext {SchemaInfo = info})
                                    )
                                )
                            );
                            break;
                        case SubQuery {RequiresIntrospection: true} when info is null:
                            throw new
                                InvalidOperationException(
                                    "Cannot build without introspection! A node requires query introspection.");
                        case SubQuery {RequiresIntrospection: true} subQuery:
                            subQuery.Build(writer, info);
                            break;
                        default:
                            QueryUtils.ParseObject(writer, Value);
                            break;
                    }
                })
            ));
}
