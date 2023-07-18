using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Builders.Wrappers
{
    internal interface IWrapper
    {
        static readonly ConcurrentBag<IWrapper> _wrappers = new()
        {
            new FSharpOptionWrapper(),
            new NullableWrapper()
        };

        static bool TryGetWrapper(Type t, [NotNullWhen(true)] out IWrapper? wrapper)
            => (wrapper = _wrappers.FirstOrDefault(x => x.IsWrapping(t))) is not null;

        /// <summary>
        ///     Returns the inner (real) type that the wrapper wraps. For example:
        ///     <see cref="Nullable"/>&lt;<see cref="int"/>&gt; would return <see cref="int"/>;
        /// </summary>
        /// <param name="wrapperType">The wrapper type to extract the inner type from.</param>
        /// <returns>The inner type.</returns>
        Type GetInnerType(Type wrapperType);

        /// <summary>
        ///     Determines whether or not this wrapper can work with the provided
        ///     wrapping type.
        /// </summary>
        /// <param name="t">
        ///     The type that is checked for a wrapper that this instance can work with.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the provided type matches the type this wrapper
        ///     can work with; otherwise <see langword="false"/>.
        /// </returns>
        bool IsWrapping(Type t);

        /// <summary>
        ///     Wraps a given value into the target type.
        /// </summary>
        /// <param name="target">The target type of the wrap.</param>
        /// <param name="value">The value to wrap.</param>
        /// <returns>The wrapped value.</returns>
        object? Wrap(Type target, object? value);
    }
}
