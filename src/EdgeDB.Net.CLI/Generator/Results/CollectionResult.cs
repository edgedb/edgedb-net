using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    internal class CollectionResult : IQueryResult
    {
        public string FileName { get; }

        public string FilePath { get; }

        private readonly IQueryResult _child;
        private readonly bool _asArray;

        public CollectionResult(string path, CodecTypeInfo info)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            if (info.Children is null || !info.Children.Any())
                throw new ArgumentException("Expected inner type of collection, but got nothing");

            _child = info.Children.First().Build(path);
            _asArray = info.Type is CodecType.Array;
        }

        public void Visit(ResultVisitor visitor)
        {
            _child.Visit(visitor);
        }

        public string ToCSharp()
        {
            return _asArray
                ? $"{_child.ToCSharp()}[]"
                : $"IEnumerable<{_child.ToCSharp()}>";
        }
    }
}

