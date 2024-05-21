
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

internal sealed class MarkerCollection : IEnumerable<Marker>
{
    private const int MaxStackallocSize = 1024;

    public readonly Dictionary<MarkerType, LinkedList<Marker>> MarkersByType = new();
    private readonly LinkedList<Marker> _markers = new();
    private readonly SortedDictionary<int, LinkedList<Marker>> _markersByPosition = new();
    public readonly Dictionary<string, LinkedList<Marker>> MarkersByName = new();

    public void Add(Marker marker)
    {
        _markers.AddLast(marker);

        if (!MarkersByType.TryGetValue(marker.Type, out var markersByType))
            MarkersByType[marker.Type] = markersByType = new();

        if (!_markersByPosition.TryGetValue(marker.Position, out var markersByPosition))
            _markersByPosition[marker.Position] = markersByPosition = new();

        if (!MarkersByName.TryGetValue(marker.Name, out var markersByName))
            MarkersByName[marker.Name] = markersByName = new();

        markersByPosition.AddLast(marker);
        markersByType.AddLast(marker);
        markersByName.AddLast(marker);
    }

    public void Remove(Range range)
    {
        var rangeLower = range.Start.Value;
        var rangeUpper = range.Start.Value + range.End.Value;

        foreach (var marker in _markers)
        {
            if (!marker.IsAlive)
                continue;

            var markerLower = marker.Position;
            var markerUpper = marker.Position + marker.Size;

            // if the marker is at the range, remove it
            if (marker.Range.Equals(range))
            {
                marker.Kill();
            }
            // remove the size from the marker
            else if (markerLower <= rangeLower && markerUpper >= rangeUpper)
            {
                marker.UpdateSize(-range.End.Value);
            }
            else if (markerLower > rangeUpper)
            {
                // move the position
                marker.UpdatePosition(-range.End.Value);
            }
        }
    }

    public void Move(Range from, Range to)
    {
        var offset = to.Start.Value - from.Start.Value;

        foreach (var marker in _markers)
        {
            if (!marker.IsAlive)
                continue;

            if (marker.Position <= to.Start.Value && marker.Position + marker.Size >= to.Start.Value)
            {
                // update marker size
                marker.UpdateSize(from.End.Value);
            }
            // if the marker contains the 'from' range BUT is not equal to the from range, update it to remove its size
            else if (
                marker.Position <= from.Start.Value &&
                marker.Position + marker.Size >= from.Start.Value + from.End.Value &&
                !marker.Range.Equals(from))
            {
                marker.UpdateSize(-from.End.Value);
            }

            // if the marker is apart of the moved span, update its position to the new offset
            if (marker.Position >= from.Start.Value &&
                marker.Position + marker.Size <= from.Start.Value + from.End.Value)
            {
                marker.UpdatePosition(offset);
            }
            // otherwise we move any markers towards the head by decrementing the size of the moved range
            else if(marker.Position > from.Start.Value + from.End.Value)
            {
                marker.UpdatePosition(-from.End.Value);
            }
        }
    }

    public unsafe void Update(Queue<int> inserts, int tokenCount)
    {
        if (inserts.Count == 0)
            return;

        if (_markers.Count == 0)
        {
            inserts.Clear();
            return;
        }

        // create a smaller batch of updates, 'inserts' are a queue of index-based updates each with a size of 1
        // meaning if we have for example [0, 1, 2], we can simplify to a range of 0..3 and increment each marker with
        // that range

        var deltasIndex = 0;
        byte[]? rangeRawArray = null;
        byte[]? deltasRawArray = null;

        var deltas = inserts.Count * sizeof(Range) <= MaxStackallocSize
            ? stackalloc Range[inserts.Count]
            : RentPrimitiveArray<Range>(inserts.Count, out rangeRawArray);
        deltas.Fill(Unsafe.As<long, Range>(ref Unsafe.AsRef(0L)));

        var deltasLookupMap = tokenCount * sizeof(int) <= MaxStackallocSize
            ? stackalloc int[tokenCount]
            : RentPrimitiveArray<int>(tokenCount, out deltasRawArray);
        deltasLookupMap.Fill(-1);

        while (inserts.TryDequeue(out var pos))
        {
            var lookup = deltasLookupMap[pos == 0 ? pos : pos - 1];

            if (lookup != -1)
            {
                ref var range = ref deltas[lookup];
                IncrementRangeEnd(ref range);
                deltasLookupMap[pos] = lookup;
            }
            else
            {
                var newDeltaIndex = deltasIndex++;
                deltas[newDeltaIndex] = new Range(pos, 1);
                deltasLookupMap[pos] = newDeltaIndex;
            }
        }

        if(deltas.Length > deltasIndex)
            deltas = deltas[..deltasIndex];

        foreach (var marker in _markers)
        {
            if(!marker.IsAlive)
                continue;

            var oldPos = marker.Position;

            foreach (var delta in deltas)
            {
                if(delta.Start.Value > marker.Position)
                    continue;

                marker.UpdatePosition(delta.End.Value);
            }

            if (oldPos != marker.Position && _markersByPosition.Remove(oldPos, out var oldMarkersByPosition))
            {
                UpdateMarkerBucket(oldMarkersByPosition, marker.Position);
            }
        }

        if (rangeRawArray is not null) ArrayPool<byte>.Shared.Return(rangeRawArray);
        if (deltasRawArray is not null) ArrayPool<byte>.Shared.Return(deltasRawArray);
    }

    private void UpdateMarkerBucket(LinkedList<Marker> bucket, int newPos)
    {
        if (!_markersByPosition.TryGetValue(newPos, out var existing))
            existing = bucket;
        else foreach (var oldMarker in bucket)
            existing.AddLast(oldMarker);

        _markersByPosition[newPos] = existing;
    }

    private static unsafe Span<T> RentPrimitiveArray<T>(int elementCount, out byte[] rawArray) where T : unmanaged
    {
        var bSize = sizeof(T) * elementCount;
        rawArray = ArrayPool<byte>.Shared.Rent(bSize);
        return new Span<T>(Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(rawArray)), elementCount);
    }

    private static unsafe void IncrementRangeEnd(ref Range range)
    {
        ref var end = ref Unsafe.AsRef<int>((byte*)Unsafe.AsPointer(ref range) + sizeof(int));
        end++;
    }

    public IEnumerable<Marker> GetStartingAt(LooseLinkedList<Value>.Node node)
        => _markers.Where(x => x.IsAlive && x.Slice.Head == node);

    public IEnumerable<Marker> GetSiblings(Marker marker)
        => _markersByPosition[marker.Position].Where(x => x != marker && x.Size == marker.Size && x.IsAlive);

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
            foreach (var minStartMarker in _markersByPosition[marker.Position].Where(x => x.Size == minStart.Size && x.IsAlive))
            {
                result.AddLast(minStartMarker);
            }
        }

        var minEnd = _markers
            .Where(x => x.Position + x.Size == marker.Position + marker.Size && x.IsAlive)
            .MinBy(x => x.Size);

        if (minEnd is not null)
        {
            foreach (var minEndMarker in _markersByPosition[minEnd.Position].Where(x => x.Size == minEnd.Size && x.IsAlive))
            {
                result.AddLast(minEndMarker);
            }
        }

        return result;
    }

    public IEnumerable<Marker> GetParents(Marker marker)
        => _markers.Where(x => x.Position < marker.Position && x.Position + x.Size > marker.Position + marker.Size);

    public IEnumerable<Marker> GetChildren(Marker marker)
        => _markers.Where(x => x.Position > marker.Position && x.Position + x.Size < marker.Position + marker.Size);

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
        MarkersByType.Clear();
        MarkersByName.Clear();
    }

    public IEnumerator<Marker> GetEnumerator() => _markers.Where(x => x.IsAlive).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
