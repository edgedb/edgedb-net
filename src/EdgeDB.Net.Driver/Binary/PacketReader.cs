using EdgeDB.Binary.Packets;
using EdgeDB.Utils;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EdgeDB.Binary
{
    internal unsafe struct PacketReader
    {
        public bool Empty
            => Contract.IsEmpty;

        internal int Limit
        {
            get => _limit;
            set
            {
                if(value < 0)
                {
                    // reset limit
                    _limit = Contract.Length;
                    return;
                }

                if (Contract.Position + value > Contract.Length)
                    throw new InternalBufferOverflowException($"Setting the buffer limit to {value} would overflow the internal buffer");

                _limit = Contract.Position + value;
            }
        }

        public int Position
            => Contract.Position;

        public int Length
            => Contract.Length;

        internal readonly PacketContract Contract;

        private int _limit;

        public PacketReader(PacketContract contract)
        {
            Contract = contract;
            _limit = Contract.Length;
        }

        public static PacketReader CreateFrom(byte[] data)
            => CreateFrom(data.AsSpan());

        public static PacketReader CreateFrom(Span<byte> data)
        {
            return new PacketReader(new PacketContract(data));
        } 

        private void VerifyInLimits(int sz)
        {
            var target = Contract.Position + sz;
            if (target > _limit || target > Contract.Length)
                throw new InternalBufferOverflowException("The requested read operation would overflow the buffer");
        }

        public ReservedBuffer* ReserveRead(uint size)
            => ReserveRead((int)size);

        public ReservedBuffer* ReserveRead(int size)
            => Contract.ReserveRead(size);

        public ReservedBuffer* ReserveReadRemaining()
            => Contract.ReserveRead(Contract.Length - Contract.Position);

        public ReservedBuffer* ReserveRead()
        {
            var length = ReadUInt32();
            return ReserveRead(length);
        }

#region Unmanaged basic reads & endianness correction
        /// <summary>
        ///     Reads an unmanaged type from the underlying contract source.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to read.</typeparam>
        /// <returns>
        ///     A reference to <typeparamref name="T"/> that can point to memory shared within
        ///     the contract source. Esentially, the callee should process <typeparamref name="T"/>
        ///     before making another call to <see cref="Read"/>. 
        /// </returns>
        private ref T Read<T>()
            where T : unmanaged
        {
            VerifyInLimits(sizeof(T));

            ref var result = ref Contract.Request<T>();

            BinaryUtils.CorrectEndianness(ref result);

            return ref result;
        }
        
        public bool ReadBoolean()
            => ReadByte() > 0;

        public ref byte ReadByte()
            => ref Read<byte>();

        public ref char ReadChar()
            => ref Read<char>();

        public ref double ReadDouble()
            => ref Read<double>();

        public ref float ReadSingle()
            => ref Read<float>();

        public ref ulong ReadUInt64()
            => ref Read<ulong>();

        public ref long ReadInt64()
            => ref Read<long>();

        public ref uint ReadUInt32()
            => ref Read<uint>();

        public ref int ReadInt32()
            => ref Read<int>();

        public ref ushort ReadUInt16()
            => ref Read<ushort>();

        public ref short ReadInt16()
            => ref Read<short>();
#endregion

        public void Skip(int count)
        {
            VerifyInLimits(count);
            Contract.Skip(count);
        }

        public Span<byte> ConsumeByteArray()
        {
            return Contract.RequestBuffer(_limit - Contract.Position);
        }

        public string ConsumeString()
            => Encoding.UTF8.GetString(ConsumeByteArray());

        public Guid ReadGuid()
        {
            VerifyInLimits(sizeof(Guid));

            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();

            var buffer = Contract.RequestBuffer(8);

            return new Guid(a, b, c, buffer.ToArray());
        }
        
        public KeyValue[] ReadKeyValues()
        {
            VerifyInLimits(sizeof(ushort));
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
            VerifyInLimits(sizeof(ushort));

            var code = ReadUInt16();


            var value = ReadUnsafeBytes().ToArray();

            return new KeyValue(code, value);
        }

        public string ReadString()
        {
            VerifyInLimits(sizeof(int));

            var strLength = (int)ReadUInt32();

            VerifyInLimits(strLength);

            var stringBuffer = Contract.RequestBuffer(strLength);

            return Encoding.UTF8.GetString(stringBuffer);
        }

        public Annotation[] ReadAnnotations()
        {
            VerifyInLimits(sizeof(ushort));
            var count = ReadUInt16();

            Annotation[] arr = new Annotation[count];

            for (ushort i = 0; i != count; i++)
            {
                arr[i] = ReadAnnotation();
            }

            return arr;
        }

        /// <summary>
        ///     Reads a buffer from the underlying contract, the buffer can point into
        ///     memory shared within the contract, making it unsafe.
        /// </summary>
        /// <remarks>
        ///     Callees should process the returned span before continuing to make read
        ///     calls.
        /// </remarks>
        private Span<byte> ReadUnsafeBytes()
        {
            var length = (int)ReadUInt32();

            return Contract.RequestBuffer(length, copy: false);
        }

        public Annotation ReadAnnotation()
        {
            var name = ReadString();
            var value = ReadString();

            return new Annotation(name, value);
        }
    }
}
