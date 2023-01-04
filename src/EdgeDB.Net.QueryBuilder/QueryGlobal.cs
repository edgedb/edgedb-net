using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a globally defined variables contained within a 'WITH' statement.
    /// </summary>
    internal class QueryGlobal
    {
        /// <summary>
        ///     Gets the value that was included in the 'WITH' statement.
        /// </summary>
        public object? Value { get; init; }

        /// <summary>
        ///     Gets the object reference that the <see cref="Value"/> represents.
        ///     For example the following code
        ///     <code>
        ///     QueryBuilder.Insert(new Person {..., Friend = new Person {...}})
        ///     </code>
        ///     would cause the nested person object to be converted to a <see cref="QueryGlobal"/>
        ///     and this property would be the actual reference to that person instance.
        /// </summary>
        public object? Reference { get; init; }

        /// <summary>
        ///     Gets the name of the global.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        ///     Constructs a new <see cref="QueryGlobal"/>.
        /// </summary>
        /// <param name="name">The name of the global.</param>
        /// <param name="value">The value which will be the assignment of this global.</param>
        public QueryGlobal(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     Constructs a new <see cref="QueryGlobal"/>.
        /// </summary>
        /// <param name="name">The name of the global.</param>
        /// <param name="value">The value which will be the assignment of this global.</param>
        /// <param name="reference">The refrence object that caused this global to be created.</param>
        public QueryGlobal(string name, object? value, object? reference)
        {
            Name = name;
            Value = value;
            Reference = reference;
        }
    }
}
