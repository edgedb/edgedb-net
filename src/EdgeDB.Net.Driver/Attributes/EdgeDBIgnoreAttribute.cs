namespace EdgeDB;

/// <summary>
///     Marks the current target to be ignored when deserializing or building queries.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field |
                AttributeTargets.Constructor)]
public class EdgeDBIgnoreAttribute : Attribute
{
}
