using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     Represents a collection result returned from a query.
    /// </summary>
    internal class CollectionResult : IQueryResult
    {
        /// <inheritdoc/>
        public string FileName { get; }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <summary>
        ///     The inner type of the collection.
        /// </summary>
        private readonly IQueryResult _child;

        /// <summary>
        ///     Whether or not to write this collection as an array when calling <see cref="ToCSharp"/>.
        /// </summary>
        private readonly bool _asArray;

        /// <summary>
        ///     Constructs a new <see cref="CollectionResult"/>.
        /// </summary>
        /// <param name="path">The file path of the edgeql query that returns this result.</param>
        /// <param name="info">The codec info used to construct this result.</param>
        /// <exception cref="ArgumentException">The inner type of the collection was missing.</exception>
        public CollectionResult(string path, CodecTypeInfo info)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            if (info.Children is null || !info.Children.Any())
                throw new ArgumentException("Expected inner type of collection, but got nothing");

            _child = info.Children.First().Build(path);
            _asArray = info.Type is CodecType.Array;
        }

        /// <inheritdoc/>
        public void Visit(ResultVisitor visitor)
        {
            _child.Visit(visitor);
        }

        /// <inheritdoc/>
        public string ToCSharp()
        {
            return _asArray
                ? $"{_child.ToCSharp()}[]"
                : $"IEnumerable<{_child.ToCSharp()}>";
        }
    }
}

