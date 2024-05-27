using EdgeDB.QueryNodes;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB;

internal sealed class GlobalReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer)
    {
        if (!writer.Markers.MarkersByType.TryGetValue(MarkerType.QueryNode, out var nodes))
            return;

        var withNode = nodes.FirstOrDefault(x => (x.Metadata as QueryNodeMetadata)?.Node is WithNode);

        if (withNode is null)
            return;

        int reducedCount = 0;
        foreach (var (_, markers) in writer.Markers.MarkersByType.Where(x => x.Key is MarkerType.GlobalDeclaration)
                     .ToArray())
        foreach (var global in markers)
        {
            if (global.Metadata is not GlobalMetadata metadata)
                continue;

            var references = writer.Markers.MarkersByType
                .Where(x => x.Key is MarkerType.GlobalReference)
                .SelectMany(x => x.Value).Where(x => x.Name == global.Name)
                .ToArray();

            if (references.Length is not 1 || !CanReduceWithNestedTypeSafety(metadata.Global, references[0], writer))
                continue;

            // inline the global.
            references[0].Move(global.Slice, global.Position..global.Size);
            global.Kill();
            reducedCount++;
        }

        // if theres nothing in the with block, we can remove it.
        if (!writer.Markers.GetChildren(withNode).Any(x => x.Type is MarkerType.GlobalDeclaration))
        {
            withNode.Remove();
            withNode.Kill();
        }
    }

    private static bool CanReduceWithNestedTypeSafety(QueryGlobal global, Marker marker, QueryWriter writer)
    {
        var bannedTypes = global switch
        {
            {Reference: IQueryBuilder builder} => builder.Nodes.Select(x => x.GetOperatingType()).ToHashSet(),
            {Value: IQueryBuilder builder} => builder.Nodes.Select(x => x.GetOperatingType()).ToHashSet(),
            _ => null
        };

        if (bannedTypes is null)
            return false;

        var nodes = writer.Markers.GetParents(marker).Where(x => x.Type is MarkerType.QueryNode);


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
