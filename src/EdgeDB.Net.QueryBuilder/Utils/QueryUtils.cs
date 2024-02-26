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
        /// <param name="writer">The writer to append the result to.</param>
        /// <param name="obj">The object to parse.</param>
        internal static void ParseObject(QueryWriter writer, object? obj)
        {
            switch (obj)
            {
                case null:
                    writer.Append("{}");
                    return;
                case Enum enm:
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
                    return;
                }
            }

            switch (obj)
            {
                case SubQuery subQuery:
                    if (subQuery.RequiresIntrospection)
                        throw new InvalidOperationException("Subquery required introspection to build");

                    subQuery.Query?.Invoke(writer);
                    break;
                case string str:
                    writer.SingleQuoted(str);
                    break;
                case char ch:
                    writer.SingleQuoted(ch);
                    break;
                case Type type:
                    writer.Append(EdgeDBTypeUtils.TryGetScalarType(type, out var info)
                        ? info.ToString()
                        : type.GetEdgeDBTypeName());
                    break;
                default:
                    writer.Append(obj.ToString());
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
