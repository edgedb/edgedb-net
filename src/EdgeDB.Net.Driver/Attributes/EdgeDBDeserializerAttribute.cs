namespace EdgeDB;

/// <summary>
///     Marks the current method as the method to use to deserialize the current type.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class EdgeDBDeserializerAttribute : Attribute
{
}
