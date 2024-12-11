using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class NestedSelectReducer : IReducer
{
    /// <summary>
    ///     Reduces sub-query selects if grammatical rules allow it.<br />
    ///     An example of this would be the following:
    ///     <c>
    ///         select (select Person)
    ///     </c>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="writer"></param>
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        if (!writer.Terms.TermsByType.TryGetValue(TermType.QueryNode, out var nodes))
            return;

        foreach (var node in nodes)
        {
            if (node.Metadata is not QueryNodeMetadata {Node: SelectNode selectNode})
                continue;

            // get the start token of the selects operand
            if (!(node.Slice.Head?.Value.Equals("select ") ?? false) || node.Slice.Head.Next is null)
                continue;

            var operandNodes = ExtractQueryOperandNode(writer.Terms.GetStartingAt(node.Slice.Head.Next), writer);

            var operandNode = operandNodes?[^1];

            if (operandNode?.Metadata is not QueryNodeMetadata {Node: SelectNode})
                continue;

            // only reduce if the select doesn't have any shape or additional tokens
            if (node.Slice.Tail != operandNodes?[0].Slice.Tail)
                continue;

            writer.Strip(node.Slice, node.Range, operandNode.Slice, operandNode.Range);

            node.Kill();
        }
    }

    private Term[]? ExtractQueryOperandNode(IEnumerable<Term> terms, QueryWriter writer)
    {
        foreach (var term in terms)
        {
            var result = ExtractQueryOperandNode(term, writer);

            if (result is not null)
                return result;
        }

        return null;
    }

    private Term[]? ExtractQueryOperandNode(Term term, QueryWriter writer) =>
        term.Type switch
        {
            TermType.QueryNode => [term],
            TermType.SubQuery when term.Slice.Head?.Next is not null =>
            [
                term, ..ExtractQueryOperandNode(writer.Terms.GetStartingAt(term.Slice.Head.Next), writer)
            ],
            _ => null
        };
}
