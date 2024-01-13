using System.Reflection;

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
        internal static void ParseObject(QueryStringWriter writer, object? obj)
        {
            if (obj is null)
            {
                writer.Append("{}");
                return;
            }

            if (obj is Enum enm)
            {
                var type = enm.GetType();
                var att = type.GetCustomAttribute<EnumSerializerAttribute>();

                if (att is not null)
                {
                    switch (att.Method)
                    {
                        case SerializationMethod.Lower:
                            writer.SingleQuoted(obj?.ToString()?.ToLower());
                            break;
                        case SerializationMethod.Numeric:
                            writer.Append(Convert.ChangeType(obj, type.BaseType ?? typeof(int)).ToString() ?? "{}");
                            break;
                        default:
                            writer.Append("{}");
                            break;
                    }

                    return;
                }

                writer.SingleQuoted(obj.ToString());
            }

            switch (obj)
            {
                case SubQuery subQuery:
                    if (subQuery.RequiresIntrospection)
                        throw new InvalidOperationException("Subquery required introspection to build");

                    if (subQuery.Query is not null)
                        subQuery.Query(writer);
                    break;
                case string str:
                    writer.SingleQuoted(str);
                    break;
                case char ch:
                    writer.SingleQuoted(ch);
                    break;
                case Type type:
                    if (EdgeDBTypeUtils.TryGetScalarType(type, out var info))
                        writer.Append(info);
                    else
                        writer.Append(type.GetEdgeDBTypeName());
                    break;
                default:
                    writer.Append(obj);
                    break;
            }
        }

        /// <summary>
        ///     Generates a random valid variable name for use in queries.
        /// </summary>
        /// <returns>A 12 character long random string.</returns>
        public static string GenerateRandomVariableName()
            => new string(Enumerable.Repeat(VARIABLE_CHARS, 12).Select(x => x[_rng.Next(x.Length)]).ToArray());
    }
}
