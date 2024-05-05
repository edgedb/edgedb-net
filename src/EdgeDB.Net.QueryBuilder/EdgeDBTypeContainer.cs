using System.Reflection;

namespace EdgeDB;

public sealed class EdgeDBTypeContainer<T>
{
    internal static EdgeDBTypeContainer<T> Create()
    {
        if (typeof(T).GetCustomAttribute<EdgeDBTypeAttribute>() is null && !TypeBuilder.IsValidObjectType(typeof(T)))
            throw new ArgumentException($"The type '{typeof(T).Name}' is not a valid EdgeDB type");

        return null!;
    }
}
