using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    internal class TupleResult : IQueryResult
    {
        public string FileName { get; }

        public string FilePath { get; }

        private readonly IEnumerable<(string? Name, IQueryResult Value)> _elements;

        public TupleResult(string path, CodecTypeInfo info)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            _elements = info.Children!.Select(x => (x.Name, x.Build(path)));
        }

        public void Visit(ResultVisitor visitor)
        {
            foreach(var element in _elements)
            {
                element.Value.Visit(visitor);
            }
        }

        public string ToCSharp()
        {
            var builtElements = _elements.Select(x => x.Name is null ? x.Value.ToCSharp() : $"{x.Value.ToCSharp()} {x.Name}");
            return $"({string.Join(", ", builtElements)})";
        }
    }
}

