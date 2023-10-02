using System.Reflection;

namespace EdgeDB;

internal static class ReflectionUtils
{
    public static bool IsSubclassOfRawGeneric(Type generic, Type? toCheck)
    {
        while (toCheck is not null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    public static bool TryGetRawGeneric(Type generic, Type? toCheck, out Type? genericReference)
    {
        genericReference = null;

        while (toCheck is not null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                genericReference = toCheck;

                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    public static object? DynamicCast(object? entity, Type to)
        => typeof(ReflectionUtils).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(to).Invoke(null, new[] {entity});

    private static T? Cast<T>(object? entity)
        => (T?)entity;

    public static object? GetDefault(Type t)
        => typeof(ReflectionUtils).GetMethod("GetDefaultGeneric", BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(t).Invoke(null, null);

    private static object? GetDefaultGeneric<T>() => default(T);
}
