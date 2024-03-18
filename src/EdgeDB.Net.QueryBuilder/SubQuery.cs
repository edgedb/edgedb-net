using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        ///     Represents a function used to compile a sub query.
        /// </summary>
        internal delegate void SubQueryBuilder(SchemaInfo schema, QueryWriter result);

        /// <summary>
        ///     Gets the query string for this subquery.
        /// </summary>
        /// <remarks>
        ///     This property is null when <see cref="RequiresIntrospection"/> is <see langword="true"/>.
        /// </remarks>
        public WriterProxy? Query { get; init; }

        /// <summary>
        ///     Gets whether or not this query requires introspection to generate.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Builder))]
        [MemberNotNullWhen(false, nameof(Query))]
        public bool RequiresIntrospection { get; init; }

        /// <summary>
        ///     Gets the builder for this subquery.
        /// </summary>
        public SubQueryBuilder? Builder { get; init; }

        /// <summary>
        ///     Constructs a new <see cref="SubQuery"/>.
        /// </summary>
        /// <param name="builder">The builder callback to build this <see cref="SubQuery"/>.</param>
        public SubQuery(SubQueryBuilder builder)
        {
            RequiresIntrospection = true;
            Builder = builder;
        }

        /// <summary>
        ///     Constructs a new <see cref="SubQuery"/>.
        /// </summary>
        /// <param name="writer">The <see cref="QueryStringWriter"/> containing the subquery.</param>
        public SubQuery(WriterProxy writer)
        {
            Query = writer;
            RequiresIntrospection = false;
        }

        /// <summary>
        ///     Builds this <see cref="SubQuery"/> using the provided introspection.
        /// </summary>
        /// <param name="info">The introspection info to build this <see cref="SubQuery"/>.</param>
        /// <param name="result">The builder to append the compiled sub query to.</param>
        /// <returns>
        ///     A <see cref="SubQuery"/> representing the built form of this queyr.
        /// </returns>
        public void Build(SchemaInfo info, QueryWriter result)
        {
            if (RequiresIntrospection)
            {
                Builder(info, result);
            }
            else
            {
                result.Append(Query);
            }

            // return new SubQuery(Builder!(info));
        }
    }
}
