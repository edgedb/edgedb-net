using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic subquery.
    /// </summary>
    internal class SubQuery
    {
        /// <summary>
        ///     Gets the query string for this subquery.
        /// </summary>
        /// <remarks>
        ///     This property is null when <see cref="RequiresIntrospection"/> is <see langword="true"/>.
        /// </remarks>
        public string? Query { get; init; }

        /// <summary>
        ///     Gets whether or not this query requires introspection to generate.
        /// </summary>
        public bool RequiresIntrospection { get; init; }

        /// <summary>
        ///     Gets the builder for this subquery.
        /// </summary>
        public Func<SchemaInfo, string>? Builder { get; init; }

        /// <summary>
        ///     Constructs a new <see cref="SubQuery"/>.
        /// </summary>
        /// <param name="builder">The builder callback to build this <see cref="SubQuery"/> into a <see cref="string"/>.</param>
        public SubQuery(Func<SchemaInfo, string> builder)
        {
            RequiresIntrospection = true;
            Builder = builder;
        }

        /// <summary>
        ///     Constructs a new <see cref="SubQuery"/>.
        /// </summary>
        /// <param name="query">The query string of this <see cref="SubQuery"/>.</param>
        public SubQuery(string query)
        {
            Query = query;
        }

        /// <summary>
        ///     Builds this <see cref="SubQuery"/> using the provided introspection.
        /// </summary>
        /// <param name="info">The introspection info to build this <see cref="SubQuery"/>.</param>
        /// <returns>
        ///     A <see cref="SubQuery"/> representing the built form of this queyr.
        /// </returns>
        public SubQuery Build(SchemaInfo info)
        {
            return new SubQuery(Builder!(info));
        }
    }
}
