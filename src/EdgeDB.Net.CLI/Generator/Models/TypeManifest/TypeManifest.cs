using System;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.CLI.Generator.Models.TypeManifest
{
    /// <summary>
    ///     A class representing the root of the type manifest file.
    /// </summary>
    public class TypeManifest
    {
        /// <summary>
        ///     Gets or sets the types defined in the manifest.
        /// </summary>
        public TypeDefinition[] Types { get; set; } = Array.Empty<TypeDefinition>();

        /// <summary>
        ///     Attepts to get a type definition from the given function.
        /// </summary>
        /// <param name="function">The function to use as a search parameter.</param>
        /// <param name="definition">The out parameter containing the definition if found; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the type definition could be found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">The function was included in multiple type definitions.</exception>
        public bool TryGetDefinition(string function, [MaybeNullWhen(false)]out TypeDefinition definition)
        {
            definition = null;

            if (Types is null)
                return false;
            
            // check if there are one or more reference to the function
            var defs = Types.Where(x => x.Functions.Contains(function));
            if (defs.Count() > 1)
                throw new InvalidOperationException($"The function \"{function}\" is included in multiple type definitions: {string.Join(", ", defs.Select(x => x.Name))}");

            if (!defs.Any())
                return false;

            definition = defs.First();

            return true;
        }
    }
}

