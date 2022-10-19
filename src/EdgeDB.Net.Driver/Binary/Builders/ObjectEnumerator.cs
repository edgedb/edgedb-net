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
        private readonly string[] _names;
        internal readonly ICodec[] Codecs;
        private readonly int _numElements;
        internal PacketReader Reader;
        private int _pos;

        internal ObjectEnumerator(ref Span<byte> data, int position, string[] names, ICodec[] codecs)
        {
            Reader = new PacketReader(ref data, position);
            _names = names;
            Codecs = codecs;
            _pos = 0;

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
            value = Codecs[_pos].Deserialize(ref innerReader);
            _pos++;
            return true;
        }
    }
}
