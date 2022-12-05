using System;
namespace EdgeDB.CLI.Generator.Models.TypeManifest
{
    public class TypeDefinition
    {
        public string? Name { get; set; }
        public PropertyMode PropertyMode { get; set; }
        public string[] Functions { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> TypeOverrides { get; set; } = new();
    }

    public enum PropertyMode
    {
        /// <summary>
        ///     All encountered proeprties are concatinated for this type.
        /// </summary>
        All,

        /// <summary>
        ///     All shared properties between all functions are included for this type.
        /// </summary>
        Shared,

        /// <summary>
        ///     The type name is looked up in the schema, and the properties
        ///     defined in the schema are used.
        /// </summary>
        Schema
    }
}

