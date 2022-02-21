using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Codecs
{
    public interface IArgumentCodec<TType> : IArgumentCodec, ICodec<TType>
    {
        void SerializeArguments(PacketWriter writer, TType? value);
    }

    public interface IArgumentCodec
    {
        void SerializeArguments(PacketWriter writer, object? value);
        byte[] SerializeArguments(object? value)
        {
            using (var writer = new PacketWriter())
            {
                SerializeArguments(writer, value);

                writer.BaseStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    writer.BaseStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }

    public interface ICodec<TConverter> : ICodec
    {
        void Serialize(PacketWriter writer, TConverter? value);
        new TConverter? Deserialize(PacketReader reader);

        new TConverter? Deserialize(byte[] buffer)
        {
            using (var reader = new PacketReader(buffer))
            {
                return Deserialize(reader);
            }
        }

        byte[] Serialize(TConverter? value)
        {
            using (var writer = new PacketWriter())
            {
                Serialize(writer, value);

                writer.BaseStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    writer.BaseStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        // ICodec
        object? ICodec.Deserialize(PacketReader reader) => Deserialize(reader);
        void ICodec.Serialize(PacketWriter writer, object? value) => Serialize(writer, (TConverter?)value);
        Type ICodec.ConverterType => typeof(TConverter);
        bool ICodec.CanConvert(Type t) => t == typeof(TConverter);
    }

    public interface ICodec
    {
        bool CanConvert(Type t);
        Type ConverterType { get; }
        void Serialize(PacketWriter writer, object? value);
        object? Deserialize(PacketReader reader);

        object? Deserialize(byte[] buffer)
        {
            using(var reader = new PacketReader(buffer))
            {
                return Deserialize(reader);
            }
        }

        byte[] Serialize(object? value)
        {
            using(var writer = new PacketWriter())
            {
                Serialize(writer, value);

                writer.BaseStream.Position = 0;
                using (var ms = new MemoryStream())
                {
                    writer.BaseStream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private static readonly List<ICodec> _codecs;

        static ICodec()
        {
            _codecs = new();

            var codecs = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.GetInterfaces().Any(x => x.Name == "IScalarCodec`1"));

            foreach(var codec in codecs)
            {
                // create instance
                var inst = (ICodec)Activator.CreateInstance(codec)!;

                _codecs.Add(inst);
            }
        }

        static IScalarCodec<TType>? GetScalarCodec<TType>()
            => (IScalarCodec<TType>?)_codecs.FirstOrDefault(x => x.ConverterType == typeof(TType));
    }

    public interface IScalarCodec<TInner> : ICodec<TInner> { }
}
