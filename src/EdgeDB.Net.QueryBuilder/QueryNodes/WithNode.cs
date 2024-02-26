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

        /// <inheritdoc/>
        public override void FinalizeQuery(QueryWriter writer)
        {
            if (Context.Values is null || !Context.Values.Any())
                return;

            writer.Append("with ");

            for (var i = 0; i != Context.Values.Count; i++)
            {
                var global = Context.Values[i];

                writer.Append(global.Name)
                    .Append(" := ");

                global.Compile(this, writer, null, SchemaInfo);

                if (i + 1 < Context.Values.Count)
                    writer.Append(", ");
            }
        }
    }
}
