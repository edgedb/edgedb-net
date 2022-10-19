using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema.DataTypes
{
    /// <summary>
    ///     Represents the cardinality of a <see cref="Property"/>.
    /// </summary>
    internal enum Cardinality
    {
        One,
        AtMostOne,
        AtLeastOne,
        Many,
    }
    internal class Property
    {
        /// <summary>
        ///     Gets or sets the "real" cardinality of the property.
        /// </summary>
        [EdgeDBProperty("real_cardinality")]
        public Cardinality Cardinality { get; set; }

        /// <summary>
        ///     Gets or sets the name of the property.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the target id of this property.
        /// </summary>
        public Guid? TargetId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this property is a link.
        /// </summary>
        public bool IsLink { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the property is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this property is exclusive
        /// </summary>
        public bool IsExclusive { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this property is computed.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this property is read-only.
        /// </summary>
        public bool IsReadonly { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this property has a default value.
        /// </summary>
        public bool HasDefault { get; set; }
    }
}
