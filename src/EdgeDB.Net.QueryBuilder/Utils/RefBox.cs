using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EdgeDB;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type


/// <summary>
///     Represents a wrapper on top of a <c>ref</c>, persisting it by holding the pointer.
/// </summary>
/// <remarks>
///     This class by no means extends the lifetime of the underlying reference, this class is only here to provide
///     a safe wrapper on top of a pointer derived from a ref.
///     <br/><br/>
///     This class interop's with <see cref="LooseLinkedList{T}"/> which manages its own allocations, providing
///     CLR-alloc'd values will result in undefined behaviour.
/// </remarks>
/// <typeparam name="T">The underlying type of the reference.</typeparam>
[DebuggerDisplay("IsNull={IsNull} Value={Value}")]
internal sealed unsafe class RefBox<T>
    where T : struct
{
    /// <summary>
    ///     Gets whether or not the reference is null.
    /// </summary>
    public bool IsNull
        => _pointer is null;

    /// <summary>
    ///     Gets the by-ref value.
    /// </summary>
    /// <exception cref="NullReferenceException">The value is free'd and no longer valid.</exception>
    public ref T Value
    {
        get
        {
            if (_pointer is null)
                throw new NullReferenceException("Value is null");

            return ref Unsafe.AsRef<T>(_pointer);
        }
    }

    /// <summary>
    ///     Gets the underlying pointer of the referenced value.
    /// </summary>
    public T* Pointer
        => _pointer;

    private T* _pointer;

    /// <summary>
    ///     Constructs a new <see cref="RefBox{T}"/>.
    /// </summary>
    /// <param name="ptr">A pointer to wrap.</param>
    private RefBox(T* ptr)
    {
        _pointer = ptr;
    }

    /// <summary>
    ///     Creates a new <see cref="RefBox{T}"/>.
    /// </summary>
    /// <param name="value">A by-ref value to wrap.</param>
    /// <returns>A <see cref="RefBox{T}"/> wrapping the provided by-ref value.</returns>
    public static RefBox<T> From(ref T value)
    {
        if (!Allocator.IsOwned(ref value))
            throw new ArgumentException("Cannot wrap the provided value as it wasn't allocated by us.", nameof(value));

        var box = new RefBox<T>((T*) Unsafe.AsPointer(ref value));

        Allocator.AddObserver(ref value, (_) => box._pointer = null);

        return box;
    }

    /// <summary>
    ///     Sets the value of this <see cref="RefBox{T}"/>.
    /// </summary>
    /// <param name="value">The by-ref value to set.</param>
    public void Set(ref T value)
    {
        _pointer = (T*) Unsafe.AsPointer(ref value);
    }

    /// <summary>
    ///     Creates a <see cref="RefBox{T}"/> whos value is null.
    /// </summary>
    public static RefBox<T> Null => new(null);
}

#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
