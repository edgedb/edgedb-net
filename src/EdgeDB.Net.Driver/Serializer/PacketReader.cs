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
        private T UnsafeReadAs<T>()
            where T : unmanaged
        {
            var ret = Unsafe.Read<T>(Unsafe.AsPointer(ref Data[Position]));

            if(ret is long l)
            {
                var t = BinaryPrimitives.ReverseEndianness(l);
            }

            CorrectEndianness(ref ret);
            Position += sizeof(T);
            return ret;
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

        public bool ReadBoolean()
            => ReadByte() > 0;

        public byte ReadByte()
            => UnsafeReadAs<byte>();

        public char ReadChar()
            => UnsafeReadAs<char>();

        public double ReadDouble()
            => UnsafeReadAs<double>();

        public float ReadSingle()
            => UnsafeReadAs<float>();

        public ulong ReadUInt64()
            => UnsafeReadAs<ulong>();

        public long ReadInt64()
            => UnsafeReadAs<long>();

        public uint ReadUInt32()
            => UnsafeReadAs<uint>();

        public int ReadInt32()
            => UnsafeReadAs<int>();

        public ushort ReadUInt16()
            => UnsafeReadAs<ushort>();

        public short ReadInt16()
            => UnsafeReadAs<short>();
        #endregion

        public void Skip(int count)
        {
            Position += count;
        }

        public byte[] ConsumeByteArray()
        {
            return Data[Position..].ToArray();
        }

        public string ConsumeString()
        {
            return Encoding.UTF8.GetString(Data[Position..]);
        }

        public Guid ReadGuid()
        {
            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();
            
            var buffer = Data[Position..(Position + 8)];
            var guid = new Guid(a, b, c, buffer.ToArray());
            Position += 8;
            return guid;
        }
        
        public KeyValue[] ReadKeyValues()
        {
            var count = ReadUInt16();
            var arr = new KeyValue[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadKeyValue();
            }
            
            return arr;
        }

        public KeyValue ReadKeyValue()
        {
            var code = ReadUInt16();
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
            var count = ReadUInt16();

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
