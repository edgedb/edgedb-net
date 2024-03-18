using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema
{
    /// <summary>
    ///     Represents the schema info containing user-defined types.
    /// </summary>
    internal class SchemaInfo
    {
        /// <summary>
        ///     Gets a read-only collection of all user-defined types.
        /// </summary>
        public IReadOnlyCollection<ObjectType> Types { get; }

        /// <summary>
        ///     Constructs a new <see cref="SchemaInfo"/> with the given types.
        /// </summary>
        /// <param name="types">A read-only collection of user-defined types.</param>
        public SchemaInfo(IReadOnlyCollection<ObjectType?> types)
        {
            Types = types!;
        }

        /// <summary>
        ///     Attempts to get a <see cref="ObjectType"/> for the given dotnet type.
        /// </summary>
        /// <param name="type">The type to get an object type for.</param>
        /// <param name="info">
        ///     The out parameter which is the object type representing <paramref name="type"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if a matching <see cref="ObjectType"/> was found; 
        ///     otherwise <see langword="false"/>.
        /// </returns>
        public bool TryGetObjectInfo(Type type, [MaybeNullWhen(false)] out ObjectType info)
            => (info = Types.FirstOrDefault(x =>
            {
                var name = type.GetEdgeDBTypeName();
                return name == x.CleanedName || name == x.Name;
            })) != null;
    }
}
