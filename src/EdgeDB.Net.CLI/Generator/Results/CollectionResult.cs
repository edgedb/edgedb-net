using EdgeDB.CLI.Generator.Models;
using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal class CollectionResult : IQueryResult
    {
        private readonly IQueryResult _child;
        private readonly bool _asArray;

        public CollectionResult(CodecTypeInfo info)
        {
            if (info.Children is null || !info.Children.Any())
                throw new ArgumentException("Expected inner type of collection, but got nothing");

            _child = info.Children.First().Build();
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

