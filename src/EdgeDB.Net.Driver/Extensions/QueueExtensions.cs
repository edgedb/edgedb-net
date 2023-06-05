#if NET461
using System.Diagnostics.CodeAnalysis;
namespace EdgeDB
{
    internal static class QueueExtensions
    {
        public static bool TryDequeue<T>(this Queue<T> queue, [NotNullWhen(true)] out T? value)
        {
            if (queue.Count > 0)
            {
                value = queue.Dequeue()!;
                return true;
            }

            value = default(T);

            return false;
        }
    }
}
#endif
