namespace EdgeDB
{
    public struct ComputedValue<TInner> : IComputedValue
    {
        public TInner? Value { get; }

        internal QueryBuilder<TInner>? Builder { get; } = null;

        internal ComputedValue(TInner? value)
        {
            Value = value;
        }

        internal ComputedValue(TInner? value, QueryBuilder<TInner> builder)
            : this(value)
        {
            Builder = builder;
        }

        public static implicit operator ComputedValue<TInner>(TInner? value)
        {
            return new(value);
        }

        object? IComputedValue.Value => Value;
        QueryBuilder? IComputedValue.Builder => Builder;
    }

    public interface IComputedValue
    {
        public object? Value { get; }

        internal QueryBuilder? Builder { get; }
    }
}
