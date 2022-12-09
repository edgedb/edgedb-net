using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EdgeDB.Binary.Codecs
{
    internal interface IArgumentCodec<TType> : IArgumentCodec, ICodec<TType>
    {
        void SerializeArguments(ref PacketWriter writer, TType? value);
    }

    internal interface IArgumentCodec
    {
        void SerializeArguments(ref PacketWriter writer, object? value);

        byte[] SerializeArguments(object? value)
        {
            var writer = new PacketWriter();
            SerializeArguments(ref writer, value);
            return writer.GetBytes().ToArray();
        }
    }

    internal interface ICodec<TConverter> : ICodec
    {   
        void Serialize(ref PacketWriter writer, TConverter? value);

        new TConverter? Deserialize(ref PacketReader reader);

        new TConverter? Deserialize(byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }
        
        new TConverter? Deserialize(Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        byte[] Serialize(TConverter? value)
        {
            var writer = new PacketWriter();
            Serialize(ref writer, value);
            return writer.GetBytes().ToArray();
        }

        // ICodec
        object? ICodec.Deserialize(ref PacketReader reader) 
            => Deserialize(ref reader);

        void ICodec.Serialize(ref PacketWriter writer, object? value) 
            => Serialize(ref writer, (TConverter?)value);

        Type ICodec.ConverterType 
            => typeof(TConverter);

        bool ICodec.CanConvert(Type t)
            => t == typeof(TConverter);
    }

    internal interface ICodec
    {
        bool CanConvert(Type t);

        Type ConverterType { get; }

        void Serialize(ref PacketWriter writer, object? value);

        object? Deserialize(ref PacketReader reader);

        object? Deserialize(Span<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        object? Deserialize(byte[] buffer)
        {
            var reader = new PacketReader(buffer);
            return Deserialize(ref reader);
        }

        byte[] Serialize(object? value)
        {
            var writer = new PacketWriter();
            Serialize(ref writer, value);
            return writer.GetBytes().ToArray();
        }

        private static readonly List<ICodec> _scalarCodecs;
        private static readonly ConcurrentDictionary<Type, ICodec> _scalarCodecMap;
        static ICodec()
        {
            _scalarCodecs = new();
            _scalarCodecMap = new();
            
            var codecs = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(x => x.Name == "IScalarCodec`1"));

            foreach (var codec in codecs)
            {
                // create instance
                var inst = (ICodec)Activator.CreateInstance(codec)!;

                _scalarCodecs.Add(inst);
                _scalarCodecMap.TryAdd(inst.ConverterType, inst);
            }
        }

        static bool ContainsScalarCodec(Type type)
            => _scalarCodecMap.ContainsKey(type);

        static bool TryGetScalarCodec(Type type, [MaybeNullWhen(false)] out ICodec codec)
            => _scalarCodecMap.TryGetValue(type, out codec);

        static IScalarCodec<TType>? GetScalarCodec<TType>()
            => (IScalarCodec<TType>?)_scalarCodecs.FirstOrDefault(x => x.ConverterType == typeof(TType) || x.CanConvert(typeof(TType)));
    }

    internal interface IScalarCodec<TInner> : ICodec<TInner> { }

    internal interface IWrappingCodec
    {
        ICodec InnerCodec { get; }
    }

    internal interface IMultiWrappingCodec
    {
        ICodec[] InnerCodecs { get; }
    }
}
