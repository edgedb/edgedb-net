using System.Diagnostics;

namespace EdgeDB
{
    //Based on https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Nullable.cs
    /// <summary>
    ///     Represents an optional value type.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public struct Optional<T>
    {
        /// <summary>
        ///     Gets the unspecified value for <typeparamref name="T"/>.
        /// </summary>
        public static Optional<T> Unspecified => default;
        private readonly T? _value;

        /// <summary> Gets the value for this parameter. </summary>
        /// <exception cref="InvalidOperationException" accessor="get">This property has no value set.</exception>
        public T? Value
        {
            get
            {
                if (!IsSpecified)
                    throw new InvalidOperationException("This property has no value set.");
                return _value;
            }
        }
        /// <summary> Returns true if this value has been specified. </summary>
        public bool IsSpecified { get; }

        /// <summary> Creates a new Parameter with the provided value. </summary>
        public Optional(T value)
        {
            _value = value;
            IsSpecified = true;
        }

        /// <summary>
        ///     Gets the value or <see langword="default"/>{<typeparamref name="T"/>}.
        /// </summary>
        /// <returns>The value or <see langword="default"/>{<typeparamref name="T"/>}.</returns>
        public T? GetValueOrDefault() => _value;

        /// <summary>
        ///     Gets the value or the provided <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="defaultValue">
        ///     The default value of <typeparamref name="T"/> to return if the current
        ///     <see cref="Optional"/> does not have a value.
        /// </param>
        /// <returns>The <see cref="Value"/>; or <paramref name="defaultValue"/>.</returns>
        public T? GetValueOrDefault(T? defaultValue) => IsSpecified ? _value : defaultValue;

        /// <inheritdoc/>
        public override bool Equals(object? other)
        {
            if (!IsSpecified) return other == null;
            if (other == null) return false;
            return _value?.Equals(other) ?? false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => IsSpecified ? _value!.GetHashCode() : 0;

        /// <inheritdoc/>
        public override string? ToString() => IsSpecified ? _value?.ToString() : null;
        private string DebuggerDisplay => IsSpecified ? _value?.ToString() ?? "<null>" : "<unspecified>";

        public static implicit operator Optional<T>(T value) => new(value);
        public static explicit operator T?(Optional<T> value) => value.Value;
    }

    /// <summary>
    ///     Represents an optional value.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        ///     Creates an unspecified optional value.
        /// </summary>
        /// <typeparam name="T">The inner type of the optional.</typeparam>
        /// <returns>A <see cref="Optional{T}"/> with no value specified.</returns>
        public static Optional<T> Create<T>() => Optional<T>.Unspecified;

        /// <summary>
        ///     Creates an optional value.
        /// </summary>
        /// <typeparam name="T">The inner type of the optional.</typeparam>
        /// <param name="value">The value of the <see cref="Optional{T}"/>.</param>
        /// <returns></returns>
        public static Optional<T> Create<T>(T value) => new(value);

        /// <summary>
        ///     Converts the <see cref="Optional{T}"/> to a <see cref="Nullable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The inner type of the optional.</typeparam>
        /// <param name="val">The optional to convert.</param>
        /// <returns>A nullable version of the optional.</returns>
        public static T? ToNullable<T>(this Optional<T> val)
            where T : struct
            => val.IsSpecified ? val.Value : null;

        public static T? GetValueOrDefault<T>(this Optional<T> option, T? defaultValue)
            where T : struct
            => option.IsSpecified ? option.Value : defaultValue;
    }
}
