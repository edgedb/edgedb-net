using EdgeDB.Translators.Expressions;
using System.Linq.Expressions;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'WITH' node.
    /// </summary>
    internal class WithNode : QueryNode<WithContext>
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryWriter writer)
        {
            if (!Builder.QueryGlobals.Any())
                return;

            writer.Append("with ");

            for (var i = 0; i != Builder.QueryGlobals.Count; i++)
            {
                var global = Builder.QueryGlobals[i];

                writer.Append(global.Name)
                    .Append(" := ");

                global.Compile(this, writer, null, SchemaInfo);

                if (i + 1 < Builder.QueryGlobals.Count)
                    writer.Append(", ");
            }
        }
    }
}
