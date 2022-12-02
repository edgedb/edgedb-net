using EdgeDB.CLI.Generator.Models;
using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal class ClassResult : IQueryResult
    {
        public string ClassName { get; set; }

        public string? Extending { get; set; }

        public bool IsAbstract { get; set; }

        public Dictionary<string, IQueryResult> Properties { get; set; }

        public IEnumerable<string> UsedNamespaces { get; }

        public ClassResult(CodecTypeInfo classInfo)
        {
            Properties = classInfo.Children!.ToDictionary(x => x.Name!, x => x.Build());
            ClassName = classInfo.Name ?? classInfo.GetUniqueTypeName();

            UsedNamespaces = GetNamespaces(classInfo).Distinct();
        }

        private static IEnumerable<string> GetNamespaces(CodecTypeInfo typeInfo)
        {
            if (typeInfo.Namespace is not null)
                yield return typeInfo.Namespace;

            if (typeInfo.Children is not null && typeInfo.Children.Any())
            {
                foreach (var ns in typeInfo.Children.SelectMany(x => GetNamespaces(x)))
                    yield return ns;
            }

        }

        public void Visit(ResultVisitor visitor)
        {
            visitor.AddTypeGenerationTarget(this);

            foreach(var prop in Properties)
            {
                prop.Value.Visit(visitor);
            }
        }

        public string ToCSharp()
        {
            return ClassName;
        }
    }
}

