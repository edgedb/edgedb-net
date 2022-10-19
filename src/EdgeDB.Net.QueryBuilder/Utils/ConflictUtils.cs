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
        /// <param name="type">The object type to generate the conflict for.</param>
        /// <param name="hasElse">Whether or not the query has an else statement.</param>
        /// <returns>
        ///     The generated 'UNLESS CONFLICT' statement.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     The conflict statement cannot be generated because of query grammar limitations.
        /// </exception>
        public static string GenerateExclusiveConflictStatement(ObjectType type, bool hasElse)
        {
            // does the type have any object level exclusive constraints?
            if (type.Constraints?.Any(x => x.IsExclusive) ?? false)
            {
                return $"unless conflict on {type.Constraints?.First(x => x.IsExclusive).SubjectExpression}";
            }

            // does the type have a single property that is exclusive?
            if(type.Properties!.Count(x => x.Name != "id" && x.IsExclusive) == 1)
            {
                return $"unless conflict on .{type.Properties!.First(x => x.IsExclusive).Name}";
            }

            // if it doesn't have an else statement we can simply add 'UNLESS CONFLICT'
            if (!hasElse)
                return "unless conflict";

            throw new InvalidOperationException($"Cannot find a valid exclusive contraint on type {type.Name}");
        }
    }
}
