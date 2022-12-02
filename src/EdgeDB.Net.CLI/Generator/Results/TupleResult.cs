using EdgeDB.CLI.Generator.Models;
using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal class TupleResult : IQueryResult
    {
        private readonly IEnumerable<(string? Name, IQueryResult Value)> _elements;

        public TupleResult(CodecTypeInfo info)
        {
            _elements = info.Children!.Select(x => (x.Name, x.Build()));
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

