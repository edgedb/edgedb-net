using System;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB.CLI.Generator.Models.TypeManifest
{
    public class TypeManifest
    {
        public TypeDefinition[] Types { get; set; } = Array.Empty<TypeDefinition>();

        public bool TryGetDefinition(string function, [MaybeNullWhen(false)]out TypeDefinition definition)
        {
            definition = null;

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

