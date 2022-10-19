namespace EdgeDB
{
    /// <summary>
    ///     Marks this class or struct as a valid type to use when serializing/deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class EdgeDBTypeAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the module name for this type.
        /// </summary>
        public string? ModuleName { get; init; }
        internal readonly string? Name;

        /// <summary>
        ///     Marks this as a valid target to use when serializing/deserializing.
        /// </summary>
        /// <param name="name">The name of the type in the edgedb schema.</param>
        public EdgeDBTypeAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Marks this as a valid target to use when serializing/deserializing.
        /// </summary>
        public EdgeDBTypeAttribute() { }
    }
}
