namespace EdgeDB
{
    /// <summary>
    ///     Marks the current field or property as a valid target for serializing/deserializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EdgeDBPropertyAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets whether or not this member is a link.
        /// </summary>
        public bool IsLink { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this member is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this member is a computed value.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this member is read-only.
        /// </summary>
        public bool IsReadOnly { get; set; }

        internal readonly string? Name;

        /// <summary>
        ///     Marks this member to be used when serializing/deserializing.
        /// </summary>
        /// <param name="propertyName">The name of the member in the edgedb schema.</param>
        public EdgeDBPropertyAttribute(string? propertyName = null)
        {
            Name = propertyName;
        }
    }
}
