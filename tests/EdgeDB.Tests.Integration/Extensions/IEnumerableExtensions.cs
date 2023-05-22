using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    internal static class IEnumerableExtensions
    {
        public static bool ReflectionSequenceEqual(this IEnumerable left, IEnumerable right)
        {
            var gLeft = left.GetType()
                .GetInterfaces()
                .FirstOrDefault(x => x.Name == "IEnumerable`1")
                ?.GenericTypeArguments[0];

            var gRight = right.GetType()
                .GetInterfaces()
                .FirstOrDefault(x => x.Name == "IEnumerable`1")
                ?.GenericTypeArguments[0];

            if ((gLeft is null || gRight is null) || gLeft != gRight)
                return false;

            return (bool)typeof(IEnumerableExtensions)
                .GetMethod("Compare", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(gLeft)
                .Invoke(null, new object[] { left, right })!;
        }

        private static bool Compare<T>(IEnumerable<T> l, IEnumerable<T> r)
            => l.SequenceEqual(r);
    }
}
