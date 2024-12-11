using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class GlobalReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        if (!writer.Terms.TermsByType.TryGetValue(TermType.QueryNode, out var nodes))
            return;

        var withNode = nodes.FirstOrDefault(x => (x.Metadata as QueryNodeMetadata)?.Node is WithNode);

        if (withNode is null)
            return;

        foreach (var (_, terms) in writer.Terms.TermsByType.Where(x => x.Key is TermType.GlobalDeclaration)
                     .ToArray())
        foreach (var global in terms)
        {
            if (global.Metadata is not GlobalMetadata metadata)
                continue;

            var references = writer.Terms.TermsByType
                .Where(x => x.Key is TermType.GlobalReference)
                .SelectMany(x => x.Value).Where(x => x.Name == global.Name)
                .ToArray();

            if (references.Length is not 1 || !CanReduceWithNestedTypeSafety(metadata.Global, references[0], writer))
                continue;

            // inline the global.
            references[0].Move(global.Slice, global.Position..global.Size);
            global.Kill();
        }

        // if theres nothing in the with block, we can remove it.
        if (!writer.Terms.GetChildren(withNode).Any(x => x.Type is TermType.GlobalDeclaration))
        {
            withNode.Remove();
            withNode.Kill();
        }
    }

    private static bool CanReduceWithNestedTypeSafety(QueryGlobal global, Term term, QueryWriter writer)
    {
        var bannedTypes = global switch
        {
            {Reference: IQueryBuilder builder} => builder.Nodes.Select(x => x.GetOperatingType()).ToHashSet(),
            {Value: IQueryBuilder builder} => builder.Nodes.Select(x => x.GetOperatingType()).ToHashSet(),
            _ => null
        };

        if (bannedTypes is null)
            return false;

        var nodes = writer.Terms.GetParents(term).Where(x => x.Type is TermType.QueryNode);


        foreach (var node in nodes)
        {
            if (node.Metadata is not QueryNodeMetadata nodeMetadata)
                return false;

            if (bannedTypes.Contains(nodeMetadata.Node.OperatingType))
                return false;
        }

        return true;
    }
}
