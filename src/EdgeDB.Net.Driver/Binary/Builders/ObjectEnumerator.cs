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
        private readonly CodecContext _context;
        private int _pos;

        internal ObjectEnumerator(
            ref Span<byte> data,
            int position,
            string[] names,
            ICodec[] codecs,
            EdgeDBTypeDeserializeInfo? deserializerInfo,
            CodecContext context)
        {
            Reader = new PacketReader(ref data, position);
            Codecs = codecs;
            _names = names;
            _pos = 0;
            _deserializerInfo = deserializerInfo;
            _context = context;

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

            value = codec.Deserialize(ref innerReader, _context);
            _pos++;
            return true;
        }
    }
}
