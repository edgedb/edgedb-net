using EdgeDB.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Utils
{
    internal unsafe class BinaryUtils
    {
        internal static int SizeOfString(string? str)
            => str is null ? 4 : Encoding.UTF8.GetByteCount(str) + 4;
        internal static int SizeOfByteArray(byte[]? arr)
            => arr?.Length + 4 ?? 4;

        internal static int SizeOfAnnotations(Annotation[]? annotations)
            => annotations?.Sum(x => x.Size) + 2 ?? 2;
        
        internal static void CorrectEndianness<T>(ref T value)
            where T : unmanaged
        {
            // return if numeric types are already in big endianness
            if (!BitConverter.IsLittleEndian)
                return;

            var size = sizeof(T);

            switch (size)
            {
                case 2:
                    {
                        ref var shortValue = ref Unsafe.As<T, ushort>(ref value);

                        shortValue = (ushort)((shortValue >> 8) + (shortValue << 8));
                    }
                    break;

                case 4:
                    {
                        ref var intValue = ref Unsafe.As<T, uint>(ref value);

                        var s1 = intValue & 0x00FF00FFu;
                        var s2 = intValue & 0xFF00FF00u;
                        intValue =
                            // rotate right (xx zz)
                            ((s1 >> 8) | (s1 << (64 - 8))) +
                            // rotate left (ww yy)
                            ((s2 << 8) | (s2 >> (32 - 8)));
                    }
                    break;
                case 8:
                    {
                        ref var longValue = ref Unsafe.As<T, ulong>(ref value);

                        // split to 32 bit for faster thruput
                        var upper = (uint)longValue;
                        var upperS1 = upper & 0x00FF00FFu;
                        var upperS2 = upper & 0xFF00FF00u;
                        var lower = (uint)(longValue >> 32);
                        var lowerS1 = lower & 0x00FF00FFu;
                        var lowerS2 = lower & 0xFF00FF00u;

                        longValue = (((ulong)(
                                // rotate right (xx zz)
                                ((upperS1 >> 8) | (upperS1 << (64 - 8))) +
                                // rotate left (ww yy)
                                ((upperS2 << 8) | (upperS2 >> (32 - 8)))
                            )) << 32) + (
                                // rotate right (xx zz)
                                ((lowerS1 >> 8) | (lowerS1 << (64 - 8))) +
                                // rotate left (ww yy)
                                ((lowerS2 << 8) | (lowerS2 >> (32 - 8)))
                            );
                    }
                    break;

                default:
                    return;
            }
        }
    }
}
