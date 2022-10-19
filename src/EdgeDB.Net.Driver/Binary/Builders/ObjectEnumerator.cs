using EdgeDB.Binary;
using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an enumerator for creating objects.
    /// </summary>
    public ref struct ObjectEnumerator
    {
        public int Length => _names.Length;
        internal PacketReader Reader;
        internal readonly ICodec[] Codecs;
        private readonly TypeDeserializeInfo? _deserializerInfo;
        private readonly string[] _names;
        private readonly int _numElements;
        private int _pos;

        internal ObjectEnumerator(
            ref Span<byte> data,
            int position,
            string[] names,
            ICodec[] codecs,
            TypeDeserializeInfo? deserializerInfo)
        {
            Reader = new PacketReader(ref data, position);
            Codecs = codecs;
            _names = names;
            _pos = 0;
            _deserializerInfo = deserializerInfo;

            _numElements = Reader.ReadInt32();
        }

        /// <summary>
        ///     Converts this <see cref="ObjectEnumerator"/> to a <see langword="dynamic"/> object.
        /// </summary>
        /// <returns>A <see langword="dynamic"/> object.</returns>
        public object? ToDynamic()
        {
            var data = new ExpandoObject();

            while (Next(out var name, out var value))
                data.TryAdd(name, value);

            return data;
        }

        /// <summary>
        ///     Reads the next property within this enumerator.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>
        ///     <see langword="true"/> if a property was read successfully; otherwise <see langword="false"/>.
        /// </returns>
        public bool Next([NotNullWhen(true)] out string? name, out object? value)
        {
            if (_pos >= _numElements || Reader.Empty)
            {
                name = null;
                value = null;
                return false;
            }

            Reader.Skip(4);

            var length = Reader.ReadInt32();

            if (length == -1)
            {
                name = _names[_pos];
                value = null;
                _pos++;
                return true;
            }

            Reader.ReadBytes(length, out var buff);

            var innerReader = new PacketReader(ref buff);
            name = _names[_pos];
            var codec = Codecs[_pos];


            // check initialization
            InitializeCodec(name, codec);

            value = codec.Deserialize(ref innerReader);
            _pos++;
            return true;
        }

        private void InitializeCodec(string? name, ICodec codec, EdgeDBPropertyInfo? prop = null)
        {
            if (_deserializerInfo is null)
                return;

            if (!(codec is IWrappingCodec or IMultiWrappingCodec))
                return;

            if (prop is null && name is not null)
                if (!_deserializerInfo.PropertyMap.TryGetValue(name, out prop))
                    return;

            if (prop is null)
                return;

            switch (codec)
            {
                case Binary.Codecs.Object obj:
                    obj.Initialize(prop.Type);
                    break;
                case Binary.Codecs.Tuple tpl:
                    {
                        var gn = prop.Type.GetGenericArguments();

                        if (gn.Length != tpl.InnerCodecs.Length)
                            throw new NoTypeConverterException($"Cannot determine inner types of the tuple {prop.Type} for property {name ?? prop.PropertyName}");

                        for (int i = 0; i != tpl.InnerCodecs.Length; i++)
                        {
                            InitializeCodec($"{name}[{i}]", tpl.InnerCodecs[i], prop);
                        }
                    }
                    break;
                case IWrappingCodec singleWrap
                        when singleWrap.InnerCodec is Binary.Codecs.Object obj:
                    obj.Initialize(prop.Type);
                    break;
                
            }
        }
    }
}
