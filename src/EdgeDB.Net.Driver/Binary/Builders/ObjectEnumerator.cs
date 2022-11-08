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
        internal static readonly Type RefType = typeof(ObjectEnumerator).MakeByRefType();

        public int Length => _names.Length;
        internal PacketReader Reader;
        internal readonly ICodec[] Codecs;
        private readonly EdgeDBTypeDeserializeInfo? _deserializerInfo;
        private readonly string[] _names;
        private readonly int _numElements;
        private int _pos;

        internal ObjectEnumerator(
            ref Span<byte> data,
            int position,
            string[] names,
            ICodec[] codecs,
            EdgeDBTypeDeserializeInfo? deserializerInfo)
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
        public dynamic? ToDynamic()
        {
            var data = new ExpandoObject();

            while (Next(out var name, out var value))
                data.TryAdd(name, value);

            return data;
        }

        /// <summary>
        ///     Cherrypicks a property based on the name. This method uses a 'peek' style of reading.
        ///     The <see cref="Next(out string?, out object?)"/> method is uneffected from this
        ///     method.
        /// </summary>
        /// <param name="name">The property name to checrrypick.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>
        ///     <see langword="true"/> if the property was able to be read; otherwise
        ///     <see langword="false"/>.
        /// </returns>
        public bool TryCherryPick(string name, [MaybeNullWhen(false)]out object? value)
        {
            value = null;
            var idx = Array.IndexOf(_names, name);

            if (idx == -1)
                return false;

            var reader = Reader.CreateSubReader();

            for(int i = 0; i != idx; i++)
            {
                reader.Skip(4);
                reader.Skip(reader.ReadInt32());
            }

            reader.Skip(4);
            var len = reader.ReadInt32();

            reader.ReadBytes(len, out var buff);
            var codecReader = new PacketReader(buff);
            value = Codecs[idx].Deserialize(ref codecReader);
            return true;
        }
        
        /// <summary>
        ///     Flattens this <see cref="ObjectEnumerator"/> into a dictionary with keys being property
        ///     names.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> representing the objects properties.</returns>
        public IDictionary<string, object?>? Flatten()
            => (IDictionary<string, object?>?)ToDynamic();

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
                        when singleWrap.InnerCodec is Binary.Codecs.Object obj &&
                        !obj.Initialized:
                    var iface = prop.Type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    if (iface is not null)
                    {
                        obj.Initialize(iface.GetGenericArguments()[0]);
                    }
                    else
                        throw new NoTypeConverterException($"Cannot determine inner type from {prop.Type}: Is it assignable to IEnumerable<T>?");
                    break;
            }
        }
    }
}
