using System;
namespace EdgeDB.CLI.Generator.Models.TypeManifest
{
    /// <summary>
    ///     A class representing a type definition in the type manifest file.
    /// </summary>
    public class TypeDefinition
    {
        /// <summary>
        ///     Gets or sets the name of the type to generate.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the property mode used to genereate the properties of this type.
        /// </summary>
        public PropertyMode PropertyMode { get; set; }

        /// <summary>
        ///     Gets or sets the functions whos result is this type.
        /// </summary>
        public string[] Functions { get; set; } = Array.Empty<string>();

        /// <summary>
        ///     Gets or sets the property type overrides for this type.
        /// </summary>
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
    }
}

