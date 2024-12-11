using EdgeDB.Translators.Expressions;
using System.Linq.Expressions;

namespace EdgeDB.QueryNodes;

/// <summary>
///     Represents a 'WITH' node.
/// </summary>
internal class WithNode : QueryNode<WithContext>
{
    /// <inheritdoc />
    public WithNode(NodeBuilder builder) : base(builder) { }

    public override void Visit()
    {
        if (Context.ValuesExpression is null) return;

        var inits = InitializationTranslator.PullInitializationExpression(Context.ValuesExpression.Body);

        Context.Values ??= inits.Any() ? new List<QueryGlobal>() : null;

        foreach (var global in inits)
        {
            Context.Values!.Add(SetGlobal(
                global.Key.Name,
                new SubQuery(writer => TranslateExpression(Context.ValuesExpression, global.Value, writer)),
                global.Value
            ));
        }
    }

    /// <inheritdoc />
    public override void FinalizeQuery(QueryWriter writer)
    {
        if (!Builder.QueryGlobals.Any())
            return;

        var groups = Builder.QueryGlobals.GroupBy(x => x.Reference).ToArray();

        writer.Append("with ");

        for (var i = 0; i != groups.Length; i++)
        {
            if (i > 0 && i < groups.Length)
                writer.Append(", ");

            var globalGroup = groups[i];

            // basic global
            QueryGlobal global;
            if (globalGroup.Count() == 1)
            {
                global = globalGroup.First();

                writer.Term(
                    TermType.BinaryOp,
                    "with_assignment",
                    Defer.This(() => $"Single global assignment: {global.Name}"),
                    new BinaryOpMetadata(ExpressionType.Assign),
                    Token.Of(writer =>
                    {
                        writer.Append(global.Name, " := ");
                        global.Compile(this, writer, CompileContext.SubQueryContext(SchemaInfo, null, writer.IsDebug),
                            SchemaInfo);
                    })
                );

                continue;
            }

            global = globalGroup.First();
            var followers = globalGroup.Skip(1);

            writer.Term(
                TermType.BinaryOp,
                "with_assignment",
                Defer.This(() => $"Global group assignment: {global.Name} ({globalGroup.Count()})"),
                new BinaryOpMetadata(ExpressionType.Assign),
                Token.Of(writer =>
                {
                    writer.Append(global.Name, " := ");
                    global.Compile(this, writer, CompileContext.SubQueryContext(SchemaInfo, null, writer.IsDebug),
                        SchemaInfo);
                })
            );

            foreach (var follower in followers)
            {
                if (!writer.Terms.TermsByType.TryGetValue(TermType.GlobalReference, out var terms))
                {
                    throw new InvalidOperationException(
                        $"The global {follower.Name} mimics another global, but this one doesn't have any references");
                }

                foreach (var term in terms.ToArray())
                {
                    term.Replace(Token.Of(writer => writer
                        .Term(
                            TermType.GlobalReference,
                            $"{global.Name}_follower_{follower.Name}",
                            Defer.This(() => $"Term is a follower of {global.Name} by reference"),
                            metadata: null,
                            global.Name
                        )
                    ));
                }
            }
        }
    }
}
