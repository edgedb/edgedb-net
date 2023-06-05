namespace System.Runtime.CompilerServices
{
    public static class RuntimeHelpers
    {
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(array.Length);
            if (length == 0)
                return Array.Empty<T>();
            T[] dest;
            if (typeof(T).IsValueType || typeof(T[]) == array.GetType())
            {
                // We know the type of the array to be exactly T[] or an array variance
                // compatible value type substitution like int[] <-> uint[].

                if (length == 0)
                {
                    return Array.Empty<T>();
                }

                dest = new T[length];
            }
            else
            {
                // The array is actually a U[] where U:T. We'll make sure to create
                // an array of the exact same backing type. The cast to T[] will
                // never fail.

                dest = (T[])(Array.CreateInstance(array.GetType().GetElementType()!, length));
            }
            Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}
