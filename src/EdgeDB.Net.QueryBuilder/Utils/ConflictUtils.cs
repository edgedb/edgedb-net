using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     A class of utility functions to help with conflict statements.
    /// </summary>
    internal static class ConflictUtils
    {
        /// <summary>
        ///     Generates an 'UNLESS CONFLICT [ON expr]' statement for the given object type.
        /// </summary>
        /// <param name="writer">The query string writer to append the exclusive constraint to.</param>
        /// <param name="type">The object type to generate the conflict for.</param>
        /// <param name="hasElse">Whether or not the query has an else statement.</param>
        /// <returns>
        ///     The generated 'UNLESS CONFLICT' statement.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     The conflict statement cannot be generated because of query grammar limitations.
        /// </exception>
        public static void GenerateExclusiveConflictStatement(QueryWriter writer, ObjectType type, bool hasElse)
        {
            // does the type have any object level exclusive constraints?
            if (type.Constraints?.Any(x => x.IsExclusive) ?? false)
            {
                writer
                    .Append("unless conflict on ")
                    .Append(type.Constraints?.First(x => x.IsExclusive).SubjectExpression);
                return;
            }

            // does the type have a single property that is exclusive?
            if(type.Properties!.Count(x => x.Name != "id" && x.IsExclusive) == 1)
            {
                writer
                    .Append("unless conflict on .")
                    .Append(type.Properties!.First(x => x.Name != "id" && x.IsExclusive).Name);

                return;
            }

            // if it doesn't have an else statement we can simply add 'UNLESS CONFLICT'
            if (!hasElse)
            {
                writer.Append("unless conflict");
                return;
            }

            throw new InvalidOperationException($"Cannot find a valid exclusive constraint on type {type.Name}");
        }
    }
}
