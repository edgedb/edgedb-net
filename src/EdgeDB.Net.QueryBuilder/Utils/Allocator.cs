using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB;

internal static unsafe class Allocator
{
    public delegate void FreeObserver(void* ptr);

    private static readonly HashSet<nuint> Allocs = [];
    private static readonly Dictionary<nuint, List<FreeObserver>> FreeObservers = [];

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public static ref T Allocate<T>()
        where T : struct
    {
        var ptr = NativeMemory.AllocZeroed((nuint)sizeof(T));

        Allocs.Add((nuint)ptr);

        return ref Unsafe.AsRef<T>(ptr);
    }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

    public static bool IsOwned<T>(ref T value)
        where T : struct
        => IsOwned(Unsafe.AsPointer(ref value));

    public static bool IsOwned(void* ptr)
        => Allocs.Contains((nuint)ptr);

    public static void Free<T>(ref T value)
        => Free(Unsafe.AsPointer(ref value));

    public static void Free(void* ptr)
    {
        if (!Allocs.Remove((nuint)ptr))
            return;

        if (FreeObservers.TryGetValue((nuint)ptr, out var observers))
        {
            foreach (var observer in observers)
            {
                observer(ptr);
            }

            observers.Clear();
            FreeObservers.Remove((nuint)ptr);
        }

        NativeMemory.Free(ptr);
    }

    public static void AddObserver<T>(ref T value, FreeObserver observer)
        => AddObserver(Unsafe.AsPointer(ref value), observer);

    public static void AddObserver(void* ptr, FreeObserver observer)
    {
        var key = (nuint)ptr;

        if (!FreeObservers.TryGetValue(key, out var observers))
            FreeObservers[key] = observers = new();

        observers.Add(observer);
    }
}
