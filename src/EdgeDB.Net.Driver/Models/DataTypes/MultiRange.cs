using EdgeDB.DataTypes;
using System.Collections;

namespace EdgeDB.Models.DataTypes;

/// <summary>
///     Represents the <c>multirange</c> type in EdgeDB.
/// </summary>
/// <typeparam name="T">The inner type of the multirange.</typeparam>
public readonly struct MultiRange<T> : IEnumerable<Range<T>>
    where T : struct
{
    /// <summary>
    ///     Gets the length of this multirange.
    /// </summary>
    public int Length
        => _ranges.Length;

    /// <summary>
    ///     Gets a <see cref="Range{T}"/> element within this multirange.
    /// </summary>
    /// <param name="i"></param>
    public readonly ref Range<T> this[int i]
        => ref _ranges[i];

    private readonly Range<T>[] _ranges;

    /// <summary>
    ///     Constructs a new <see cref="MultiRange{T}"/>.
    /// </summary>
    /// <param name="set">A set of ranges to put within this multirange.</param>
    public MultiRange(HashSet<Range<T>> set)
    {
        _ranges = set.ToArray();
    }

    internal MultiRange(Range<T>[] ranges)
    {
        _ranges = ranges;
    }

    #region Enumerator
    private sealed class MultiRangeEnumerator : IEnumerator<Range<T>>
    {
        private readonly MultiRange<T> _multiRange;
        private int _index;

        internal MultiRangeEnumerator(in MultiRange<T> multiRange)
        {
            _multiRange = multiRange;
            _index = -1;
        }

        public bool MoveNext()
        {
            var index = _index + 1;
            if (index >= _multiRange.Length)
            {
                _index = _multiRange.Length;
                return false;
            }
            _index = index;
            return true;
        }

        public void Reset() => _index = -1;

        public Range<T> Current
        {
            get
            {
                var index = _index;

                if (index >= _multiRange.Length)
                {
                    if (index < 0)
                    {
                        throw new InvalidOperationException("Enumeration hasn't started");
                    }
                    else
                    {
                        throw new InvalidOperationException("The enumeration has finished");
                    }
                }

                return _multiRange[index];
            }
        }

        public void Dispose()
        { }

        object IEnumerator.Current => Current;
    }
    #endregion

    /// <inheritdoc />
    public IEnumerator<Range<T>> GetEnumerator() => new MultiRangeEnumerator(in this);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _ranges.GetEnumerator();
}
