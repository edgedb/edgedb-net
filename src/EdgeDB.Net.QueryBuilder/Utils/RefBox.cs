using System.Runtime.CompilerServices;

namespace EdgeDB;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

internal sealed unsafe class RefBox<T>
    where T : struct
{
    public ref T Value
    {
        get
        {
            if (_pointer is null)
                throw new NullReferenceException("Value is null");

            return ref Unsafe.AsRef<T>(_pointer);
        }
    }

    public T* Pointer
        => _pointer;

    private T* _pointer;

    public RefBox(T* ptr)
    {
        _pointer = ptr;
        RefList.AddInvalidator(ptr, () => _pointer = null);
    }

    public RefBox(ref T value)
    {
        _pointer = (T*) Unsafe.AsPointer(ref value);
        RefList.AddInvalidator(_pointer, () => _pointer = null);
    }
}

internal static class RefList
{
    public static readonly Dictionary<IntPtr, List<Action>> InvalidateBoxes = [];

    public static unsafe void Invalidate(void* ptr)
    {
        if (!InvalidateBoxes.TryGetValue((IntPtr)ptr, out var actions))
            return;

        foreach (var action in actions)
        {
            action();
        }
    }

    public static unsafe void AddInvalidator(void* ptr, Action func)
    {
        var pointer = (IntPtr)ptr;
        if (!InvalidateBoxes.TryGetValue(pointer, out var actions))
            actions = InvalidateBoxes[pointer] = new();

        actions.Add(func);
    }
}

#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

