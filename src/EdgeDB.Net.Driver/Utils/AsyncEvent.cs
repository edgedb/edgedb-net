using System.Collections.Concurrent;

namespace EdgeDB
{
    internal class AsyncEvent<T>
        where T : class
    {
        internal ConcurrentDictionary<int, T> _subscriptions;

        public bool HasSubscribers => !_subscriptions.IsEmpty;
        public T[] Subscriptions => _subscriptions.Values.ToArray();

        public AsyncEvent()
        {
            _subscriptions = new();
        }

        public void Add(T subscriber)
            => _subscriptions.TryAdd(subscriber.GetHashCode(), subscriber);

        public void Remove(T subscriber)
            => _subscriptions.TryRemove(subscriber.GetHashCode(), out _);
    }

    internal static class EventExtensions
    {
        public static async ValueTask InvokeAsync(this AsyncEvent<Func<ValueTask>> eventHandler)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke().ConfigureAwait(false);
        }

        public static async ValueTask<T2[]> InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, ValueTask<T2>>> eventHandler, T1 arg)
        {
            var subscribers = eventHandler.Subscriptions;

            T2[] arr = new T2[subscribers.Length];

            for (int i = 0; i < subscribers.Length; i++)
                arr[i] = await subscribers[i].Invoke(arg).ConfigureAwait(false);

            return arr;
        }

        public static async ValueTask InvokeAsync<T>(this AsyncEvent<Func<T, ValueTask>> eventHandler, T arg)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke(arg).ConfigureAwait(false);
        }

        public static async ValueTask InvokeAsync<T1, T2>(this AsyncEvent<Func<T1, T2, ValueTask>> eventHandler, T1 arg1, T2 arg2)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke(arg1, arg2).ConfigureAwait(false);
        }

        public static async ValueTask InvokeAsync<T1, T2, T3>(this AsyncEvent<Func<T1, T2, T3, ValueTask>> eventHandler, T1 arg1, T2 arg2, T3 arg3)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke(arg1, arg2, arg3).ConfigureAwait(false);
        }

        public static async ValueTask InvokeAsync<T1, T2, T3, T4>(this AsyncEvent<Func<T1, T2, T3, T4, ValueTask>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke(arg1, arg2, arg3, arg4).ConfigureAwait(false);
        }

        public static async ValueTask InvokeAsync<T1, T2, T3, T4, T5>(this AsyncEvent<Func<T1, T2, T3, T4, T5, ValueTask>> eventHandler, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var subscribers = eventHandler.Subscriptions;
            for (int i = 0; i < subscribers.Length; i++)
                await subscribers[i].Invoke(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(false);
        }
    }
}
