using EdgeDB.Binary;
using EdgeDB.Models;
using EdgeDB.Utils;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EdgeDB
{
    internal unsafe ref struct PacketReader
    {
        public bool Empty
            => Position >= Data.Length || Data.IsEmpty;
        
        internal Span<byte> Data;
        internal int Position;
        
        public PacketReader(Span<byte> bytes, int position = 0)
        {
            Data = bytes;
            Position = position;
        }

        public PacketReader(ref Span<byte> bytes, int position = 0)
        {
            Data = bytes;
            Position = position;
        }

        #region Unmanaged basic reads & endianness correction
        private ref T UnsafeReadAs<T>()
            where T : unmanaged
        {
            ref var ret = ref Unsafe.As<byte, T>(ref Data[Position]);
            CorrectEndianness(ref ret);
            Position += sizeof(T);
            return ref ret;
        }

        private static void CorrectEndianness<T>(ref T value)
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
                        var upper = (uint)(longValue >> 32);
                        var upperS1 = upper & 0x00FF00FFu;
                        var upperS2 = upper & 0xFF00FF00u;
                        
                        var lower = (uint)(longValue << 32);
                        var lowerS1 = lower & 0x00FF00FFu;
                        var lowerS2 = lower & 0xFF00FF00u;

                        longValue =
                            (
                                // rotate right (xx zz)
                                ((upperS1 >> 8) | (upperS1 << (64 - 8))) +
                                // rotate left (ww yy)
                                ((upperS2 << 8) | (upperS2 >> (32 - 8)))
                            ) + (
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

        public bool ReadBoolean()
            => ReadByte() > 0;

        public ref byte ReadByte()
            => ref UnsafeReadAs<byte>();

        public ref char ReadChar()
            => ref UnsafeReadAs<char>();

        public ref double ReadDouble()
            => ref UnsafeReadAs<double>();

        public ref float ReadSingle()
            => ref UnsafeReadAs<float>();

        public ref ulong ReadUInt64()
            => ref UnsafeReadAs<ulong>();

        public ref long ReadInt64()
            => ref UnsafeReadAs<long>();

        public ref uint ReadUInt32()
            => ref UnsafeReadAs<uint>();

        public ref int ReadInt32()
            => ref UnsafeReadAs<int>();

        public ref ushort ReadUInt16()
            => ref UnsafeReadAs<ushort>();

        public ref short ReadInt16()
            => ref UnsafeReadAs<short>();
        #endregion

        public void Skip(int count)
        {
            Position += count;
        }

        public byte[] ConsumeByteArray()
        {
            return Data.ToArray();
        }

        public string ConsumeString()
        {
            return Encoding.UTF8.GetString(Data);
        }

        public Guid ReadGuid()
        {
            ref var a = ref ReadInt32();
            ref var b = ref ReadInt16();
            ref var c = ref ReadInt16();
            
            var buffer = Data[Position..(Position + 8)];
            var guid = new Guid(a, b, c, buffer.ToArray());
            Position += 8;
            return guid;
        }
        
        public KeyValue[] ReadKeyValues()
        {
            ref var count = ref ReadUInt16();
            var arr = new KeyValue[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadKeyValue();
            }
            
            return arr;
        }

        public KeyValue ReadKeyValue()
        {
            ref var code = ref ReadUInt16();
            var value = ReadByteArray();

            return new KeyValue(code, value);
        }

        public string ReadString()
        {
            var strLength = (int)ReadUInt32();
            var str = Encoding.UTF8.GetString(Data[Position..(strLength + Position)]);
            Position += strLength;
            return str;
        }

        public Annotation[] ReadAnnotations()
        {
            ref var count = ref ReadUInt16();

            Annotation[] arr = new Annotation[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadAnnotation();
            }

            return arr;
        }

        public Annotation ReadAnnotation()
        {
            var name = ReadString();
            var value = ReadString();

            return new Annotation(name, value);
        }

        public byte[] ReadByteArray()
        {
            var length = (int)ReadUInt32();
            var buffer = Data[Position..(Position + length)];
            Position += length;
            return buffer.ToArray();
        }

        public void ReadBytes(int length, out Span<byte> buff)
        {
            buff = Data[Position..(Position + length)];
            Position += length;
        }

        public void Dispose()
        {
            Data.Clear();
        }
    }
}
