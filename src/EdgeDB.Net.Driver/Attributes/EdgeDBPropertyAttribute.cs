namespace EdgeDB;

/// <summary>
///     Marks the current field or property as a valid target for serializing/deserializing.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class EdgeDBPropertyAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets whether or not the property is on a link.
    /// </summary>
    public bool IsLinkProperty { get; set; }

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
