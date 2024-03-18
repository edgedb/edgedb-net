using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema.DataTypes
{
    /// <summary>
    ///     Represents the partial 'schema::ObjectType' type within EdgeDB.
    /// </summary>
    [EdgeDBType(ModuleName = "schema")]
    internal class ObjectType
    {
        /// <summary>
        ///     Gets the cleaned name of the oject type.
        /// </summary>
        [EdgeDBIgnore]
        public string CleanedName
            => Name!.Split("::")[1];

        /// <summary>
        ///     Gets or sets the id of this object type.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the name of this object type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this object type is abstract.
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        ///     Gets or sets a collection of properties within this object type.
        /// </summary>
        [EdgeDBProperty("pointers")]
        public Property[]? Properties { get; set; }

        /// <summary>
        ///     Gets or sets a collection of constaints on the object level.
        /// </summary>
        public Constraint[]? Constraints { get; set; }
    }
}
