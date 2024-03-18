
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB;

internal sealed class MarkerCollection : IEnumerable<Marker>
{
    private readonly Dictionary<MarkerType, LinkedList<Marker>> _markersByType = new();
    private readonly LinkedList<Marker> _markers = new();
    private readonly SortedDictionary<int, LinkedList<Marker>> _markersByPosition = new();
    public readonly Dictionary<string, LinkedList<Marker>> MarkersByName = new();

    public void Add(Marker marker)
    {
        _markers.AddLast(marker);

        if (!_markersByType.TryGetValue(marker.Type, out var markersByType))
            _markersByType[marker.Type] = markersByType = new();

        if (!_markersByPosition.TryGetValue(marker.Position, out var markersByPosition))
            _markersByPosition[marker.Position] = markersByPosition = new();

        if (!MarkersByName.TryGetValue(marker.Name, out var markersByName))
            MarkersByName[marker.Name] = markersByName = new();

        markersByPosition.AddLast(marker);
        markersByType.AddLast(marker);
        markersByName.AddLast(marker);
    }

    public void Update(int position, int delta)
    {
        foreach (var marker in _markers)
        {
            if(marker.Position <= position)
                continue;

            _markersByPosition[marker.Position].Remove(marker);
            marker.Update(delta);
            if (!_markersByPosition.TryGetValue(marker.Position, out var markersByPosition))
                _markersByPosition[marker.Position] = markersByPosition = new();

            markersByPosition.AddLast(marker);
        }
    }

    public IEnumerable<Marker> GetSiblings(Marker marker)
        => _markersByPosition[marker.Position].Where(x => x != marker && x.Size == marker.Size);

    public LinkedList<Marker> GetDirectParents(Marker marker)
    {
        var result = new LinkedList<Marker>();

        var minStart = _markersByPosition[marker.Position].MinBy(x =>
        {
            var cmp = x.Size - marker.Size;
            return cmp <= 0 ? int.MaxValue : cmp;
        });

        if (minStart is not null)
        {
            foreach (var minStartMarker in _markersByPosition[marker.Position].Where(x => x.Size == minStart.Size))
            {
                result.AddLast(minStartMarker);
            }
        }

        var minEnd = _markers
            .Where(x => x.Position + x.Size == marker.Position + marker.Size)
            .MinBy(x => x.Size);

        if (minEnd is not null)
        {
            foreach (var minEndMarker in _markersByPosition[minEnd.Position].Where(x => x.Size == minEnd.Size))
            {
                result.AddLast(minEndMarker);
            }
        }

        return result;
    }

    public IEnumerable<Marker> GetParents(Marker marker)
        => _markers.Where(x => x.Position < marker.Position && x.Size > marker.Size);

    public IEnumerable<Marker> GetChildren(Marker marker)
        => _markers.Where(x => x.Position > marker.Position && x.Size < marker.Size);

    public bool TryGetNextNeighbours(Marker marker, [MaybeNullWhen(false)] out LinkedList<Marker> neighbours)
        => _markersByPosition.TryGetValue(marker.Position + marker.Size, out neighbours);

    public bool TryGetPreviousNeighbours(Marker marker, [MaybeNullWhen(false)] out LinkedList<Marker> neighbours)
        => _markersByPosition.TryGetValue(marker.Position - 1, out neighbours);

    public bool TryGetMarkersByName(string name, [MaybeNullWhen(false)] out LinkedList<Marker> markers)
        => MarkersByName.TryGetValue(name, out markers);

    public void Clear()
    {
        _markers.Clear();
        _markersByPosition.Clear();
        _markersByType.Clear();
        MarkersByName.Clear();
    }

    public IEnumerator<Marker> GetEnumerator() => _markers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_markers).GetEnumerator();
}
