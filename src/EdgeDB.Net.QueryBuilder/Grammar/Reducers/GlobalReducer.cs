using EdgeDB.QueryNodes;

namespace EdgeDB;

internal sealed class GlobalReducer : IReducer
{
    public void Reduce(IQueryBuilder builder, QueryWriter writer, Queue<IReducer> shouldRunAfter)
    {
        if (!writer.Markers.MarkersByType.TryGetValue(MarkerType.QueryNode, out var nodes))
            return;

        var withNode = nodes.FirstOrDefault(x => (x.Metadata as QueryNodeMetadata)?.Node is WithNode);

        if (withNode is null)
            return;

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
            references[0].Replace(global.Slice, global.Position..global.Size);
            global.Kill();
        }

        // if theres nothing in the with block, we can remove it.
        if (!writer.Markers.GetChildren(withNode).Any(x => x.Type is MarkerType.GlobalDeclaration))
        {
            withNode.Remove();
            withNode.Kill();
        }
    }

    private bool CanReduceWithNestedTypeSafety(QueryGlobal global, Marker marker, QueryWriter writer)
    {
        // TODO:
        // we cant reduce a global when:
        // - is a query builder inside of a nested query that selects the same type.

        return true;
    }
}
