using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EdgeDB.Binary
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TransientTemporal
    {
        public readonly long A;
        public readonly short B;

        public DateTime DateTime
            => AsUnsafe<DateTime>();

        public DateTimeOffset DateTimeOffset
            => AsUnsafe<DateTimeOffset>();

        public TimeSpan TimeSpan
            => AsUnsafe<TimeSpan>();

        public DateOnly DateOnly
            => AsUnsafe<DateOnly>();

        public TimeOnly TimeOnly
            => AsUnsafe<TimeOnly>();

        private T AsUnsafe<T>()
            where T : unmanaged
        {
            return *(T*)Unsafe.AsPointer(ref this);
        }

        public static ref TransientTemporal From(ref TimeSpan ts)
            => ref FromUnsafe(ref ts);

        public static ref TransientTemporal From(ref DateTimeOffset dto)
            => ref FromUnsafe(ref dto);

        public static ref TransientTemporal From(ref DateTime dt)
            => ref FromUnsafe(ref dt);

        public static ref TransientTemporal From(ref DateOnly dateOnly)
            => ref FromUnsafe(ref dateOnly);

        public static ref TransientTemporal From(ref TimeOnly to)
            => ref FromUnsafe(ref to);

        private static ref TransientTemporal FromUnsafe<T>(ref T value)
            where T : unmanaged
            => ref Unsafe.As<T, TransientTemporal>(ref value);
    }
}

