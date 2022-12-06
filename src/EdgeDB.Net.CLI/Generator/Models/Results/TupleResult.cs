using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     Represents a tuple result returned from a query.
    /// </summary>
    internal class TupleResult : IQueryResult
    {
        /// <inheritdoc/>
        public string FileName { get; }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <summary>
        ///     The elements of the tuple.
        /// </summary>
        private readonly IEnumerable<(string? Name, IQueryResult Value)> _elements;

        /// <summary>
        ///     Constructs a new <see cref="TupleResult"/>.
        /// </summary>
        /// <param name="path">The file path of the edgeql query that returns this result.</param>
        /// <param name="info">The codec info used to construct this result.</param>
        public TupleResult(string path, CodecTypeInfo info)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            _elements = info.Children!.Select(x => (x.Name, x.Build(path)));
        }

        /// <inheritdoc/>
        public void Visit(ResultVisitor visitor)
        {
            foreach(var element in _elements)
            {
                element.Value.Visit(visitor);
            }
        }

        /// <inheritdoc/>
        public string ToCSharp()
        {
            var builtElements = _elements.Select(x => x.Name is null ? x.Value.ToCSharp() : $"{x.Value.ToCSharp()} {x.Name}");
            return $"({string.Join(", ", builtElements)})";
        }
    }
}

