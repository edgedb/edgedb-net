using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic object within EdgeDB.
    /// </summary>
    public sealed class EdgeDBObject
    {
        /// <summary>
        ///     Gets the unique identifier for this object.
        /// </summary>
        [EdgeDBProperty("id")]
        public Guid Id { get; }

        /// <summary>
        ///     Constructs a new <see cref="EdgeDBObject"/> with the given data.
        /// </summary>
        /// <param name="data">The raw data for this object.</param>
        [EdgeDBDeserializer]
        internal EdgeDBObject(IDictionary<string, object?> data)
        {
            Id = (Guid)data["id"]!;
        }
    }
}
