using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception thrown when no type converter could be found.
    /// </summary>
    public class NoTypeConverterException : EdgeDBException
    {
        /// <summary>
        ///     Constructs a new <see cref="NoTypeConverterException"/> with the target and source types.
        /// </summary>
        /// <param name="target">The target type that <paramref name="source"/> was going to be converted to.</param>
        /// <param name="source">The source type.</param>
        public NoTypeConverterException(Type target, Type source)
            : base($"Could not convert {source.Name} to {target.Name}", false, false)
        {

        }

        /// <summary>
        ///     Constructs a new <see cref="NoTypeConverterException"/> with the target and source type,
        ///     and inner exception.
        /// </summary>
        /// <param name="target">The target type that <paramref name="source"/> was going to be converted to.</param>
        /// <param name="source">The source type.</param>
        /// <param name="inner">The inner exception.</param>
        public NoTypeConverterException(Type target, Type source, Exception inner)
            : base($"Could not convert {source.Name} to {target.Name}", inner, false, false)
        {

        }

        /// <summary>
        ///     Constructs a new <see cref="NoTypeConverterException"/> with the specified error message.
        /// </summary>
        /// <param name="message">The error message describing why this exception was thrown.</param>
        /// <param name="inner">An optional inner exception.</param>
        public NoTypeConverterException(string message, Exception? inner = null)
            : base(message, inner)
        {

        }
    }
}
