using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

internal sealed class TermCollection : IEnumerable<Term>
{
    private const int MaxStackallocSize = 1024;
    private readonly LinkedList<Term> _terms = new();
    private readonly SortedDictionary<int, LinkedList<Term>> _termsByPosition = new();
    public readonly Dictionary<string, LinkedList<Term>> TermsByName = new();

    public readonly Dictionary<TermType, LinkedList<Term>> TermsByType = new();

    public IEnumerator<Term> GetEnumerator() => _terms.Where(x => x.IsAlive).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Term term)
    {
        _terms.AddLast(term);

        if (!TermsByType.TryGetValue(term.Type, out var termsByType))
            TermsByType[term.Type] = termsByType = new LinkedList<Term>();

        if (!_termsByPosition.TryGetValue(term.Position, out var termsByPosition))
            _termsByPosition[term.Position] = termsByPosition = new LinkedList<Term>();

        if (!TermsByName.TryGetValue(term.Name, out var termsByName))
            TermsByName[term.Name] = termsByName = new LinkedList<Term>();

        termsByPosition.AddLast(term);
        termsByType.AddLast(term);
        termsByName.AddLast(term);
    }

    public void Remove(Range range, bool preserveTerms = true)
    {
        var rangeLower = range.Start.Value;
        var rangeUpper = range.Start.Value + range.End.Value;

        foreach (var term in _terms)
        {
            if (!term.IsAlive)
                continue;

            var termLower = term.Position;
            var termUpper = term.Position + term.Size;

            // if the term is at the range, remove it
            if (term.Range.Equals(range))
            {
                if (!preserveTerms) term.Kill();
                else term.Size = 0;
            }
            // remove the size from the term
            else if (termLower <= rangeLower && termUpper >= rangeUpper)
            {
                term.UpdateSize(-range.End.Value);
            }
            else if (termLower >= rangeUpper)
            {
                // move the position
                term.UpdatePosition(-range.End.Value);
            }
        }
    }

    public void Move(Range from, Range to)
    {
        var offset = to.Start.Value - (from.Start.Value + from.End.Value);

        foreach (var term in _terms)
        {
            if (!term.IsAlive)
                continue;

            if (term.Position <= to.Start.Value && term.Position + term.Size >= to.Start.Value)
            {
                // update term size
                term.UpdateSize(from.End.Value);
            }
            // if the term contains the 'from' range BUT is not equal to the from range, update it to remove its size
            else if (
                term.Position <= from.Start.Value &&
                term.Position + term.Size >= from.Start.Value + from.End.Value &&
                !term.Range.Equals(from))
            {
                term.UpdateSize(-from.End.Value);
            }

            // if the term is apart of the moved span, update its position to the new offset
            if (term.Position >= from.Start.Value &&
                term.Position + term.Size <= from.Start.Value + from.End.Value)
            {
                term.UpdatePosition(offset);
            }
            // otherwise we move any terms towards the head by decrementing the size of the moved range
            else if (term.Position > from.Start.Value + from.End.Value)
            {
                term.UpdatePosition(-from.End.Value);
            }
        }
    }

    public unsafe void Update(Queue<int> inserts, int tokenCount)
    {
        if (inserts.Count == 0)
            return;

        if (_terms.Count == 0)
        {
            inserts.Clear();
            return;
        }

        // create a smaller batch of updates, 'inserts' are a queue of index-based updates each with a size of 1
        // meaning if we have for example [0, 1, 2], we can simplify to a range of 0..3 and increment each term with
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

        if (deltas.Length > deltasIndex)
            deltas = deltas[..deltasIndex];

        foreach (var term in _terms)
        {
            if (!term.IsAlive)
                continue;

            var oldPos = term.Position;

            foreach (var delta in deltas)
            {
                if (delta.Start.Value > term.Position)
                    continue;

                term.UpdatePosition(delta.End.Value);
            }

            if (oldPos != term.Position && _termsByPosition.Remove(oldPos, out var oldTermsByPosition))
            {
                UpdateTermBucket(oldTermsByPosition, term.Position);
            }
        }

        if (rangeRawArray is not null) ArrayPool<byte>.Shared.Return(rangeRawArray);
        if (deltasRawArray is not null) ArrayPool<byte>.Shared.Return(deltasRawArray);
    }

    private void UpdateTermBucket(LinkedList<Term> bucket, int newPos)
    {
        if (!_termsByPosition.TryGetValue(newPos, out var existing))
            existing = bucket;
        else
            foreach (var oldTerm in bucket)
                existing.AddLast(oldTerm);

        _termsByPosition[newPos] = existing;
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

    public IEnumerable<Term> GetStartingAt(LooseLinkedList<Token>.Node node)
        => _terms.Where(x => x.IsAlive && x.Slice.Head == node);

    public IEnumerable<Term> GetSiblings(Term term)
        => _termsByPosition[term.Position].Where(x => x != term && x.Size == term.Size && x.IsAlive);

    public LinkedList<Term> GetDirectParents(Term term)
    {
        var result = new LinkedList<Term>();

        var minStart = _termsByPosition[term.Position].MinBy(x =>
        {
            var cmp = x.Size - term.Size;
            return cmp <= 0 ? int.MaxValue : cmp;
        });

        if (minStart is not null)
        {
            foreach (var minStartTerm in _termsByPosition[term.Position]
                         .Where(x => x.Size == minStart.Size && x.IsAlive))
            {
                result.AddLast(minStartTerm);
            }
        }

        var minEnd = _terms
            .Where(x => x.Position + x.Size == term.Position + term.Size && x.IsAlive)
            .MinBy(x => x.Size);

        if (minEnd is not null)
        {
            foreach (var minEndTerm in _termsByPosition[minEnd.Position].Where(x => x.Size == minEnd.Size && x.IsAlive))
            {
                result.AddLast(minEndTerm);
            }
        }

        return result;
    }

    public IEnumerable<Term> GetParents(Term term)
        => _terms.Where(x =>
            x.Position != term.Position && x.Size != term.Size && x.Position <= term.Position &&
            x.Position + x.Size >= term.Position + term.Size);

    public IEnumerable<Term> GetChildren(Term term)
        => _terms.Where(x =>
            x.Position != term.Position && x.Size != term.Size &&
            x.Position >= term.Position && x.Position + x.Size <= term.Position + term.Size
        );

    public IEnumerable<Term> GetChildrenOfType(Term term, TermType type)
        => TermsByType.TryGetValue(type, out var candidates)
            ? candidates.Where(x =>
                x.Position != term.Position && x.Size != term.Size && x.Position >= term.Position &&
                x.Position + x.Size <= term.Position + term.Size)
            : Array.Empty<Term>();

    public IEnumerable<Term> GetDirectChildrenOfType(Term term, TermType type)
    {
        var children = GetChildrenOfType(term, type).ToList();

        return children.Where(child => !children.Any(x => x != child && x.Range.Contains(child.Position)));
    }


    public bool TryGetNextNeighbours(Term term, [MaybeNullWhen(false)] out LinkedList<Term> neighbours)
        => _termsByPosition.TryGetValue(term.Position + term.Size, out neighbours);

    public bool TryGetPreviousNeighbours(Term term, [MaybeNullWhen(false)] out LinkedList<Term> neighbours)
        => _termsByPosition.TryGetValue(term.Position - 1, out neighbours);

    public bool TryGetTermsByName(string name, [MaybeNullWhen(false)] out LinkedList<Term> terms)
        => TermsByName.TryGetValue(name, out terms);

    public void Clear()
    {
        _terms.Clear();
        _termsByPosition.Clear();
        TermsByType.Clear();
        TermsByName.Clear();
    }
}
