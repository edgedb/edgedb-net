using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     A class containing useful utilities for building queries.
    /// </summary>
    internal static class QueryUtils
    {
        private const string VARIABLE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random _rng = new();

        /// <summary>
        ///     Parses a given object into its equivilant edgeql form.
        /// </summary>
        /// <param name="obj">The object to parse.</param>
        /// <returns>The string representation for the given object.</returns>
        internal static string ParseObject(object? obj)
        {
            if (obj is null)
                return "{}";

            if (obj is Enum enm)
            {
                var type = enm.GetType();
                var att = type.GetCustomAttribute<EnumSerializerAttribute>();
                return att != null ? att.Method switch
                {
                    SerializationMethod.Lower => $"\"{obj.ToString()?.ToLower()}\"",
                    SerializationMethod.Numeric => Convert.ChangeType(obj, type.BaseType ?? typeof(int)).ToString() ?? "{}",
                    _ => "{}"
                } : $"\"{obj}\"";
            }

            return obj switch
            {
                SubQuery query when !query.RequiresIntrospection => query.Query!,
                string str => $"\"{str}\"",
                char chr => $"\"{chr}\"",
                Type type => EdgeDBTypeUtils.TryGetScalarType(type, out var info) ? info.ToString() : type.GetEdgeDBTypeName(),
                _ => obj.ToString()!
            };
        }

        /// <summary>
        ///     Generates a random valid variable name for use in queries.
        /// </summary>
        /// <returns>A 12 character long random string.</returns>
        public static string GenerateRandomVariableName()
            => new string(Enumerable.Repeat(VARIABLE_CHARS, 12).Select(x => x[_rng.Next(x.Length)]).ToArray());
    }
}
