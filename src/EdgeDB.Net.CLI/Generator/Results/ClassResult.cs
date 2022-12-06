using EdgeDB.CLI.Generator.Models;
using EdgeDB.Utils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EdgeDB.CLI.Generator.Results
{
    internal class ClassResult : IQueryResult
    {
        public string FileName { get; }

        public string FilePath { get; }

        public string ClassName { get; set; }

        public string? Extending { get; set; }

        public bool IsAbstract { get; set; }

        public Dictionary<string, IQueryResult> Properties { get; set; }

        public IEnumerable<string> UsedNamespaces { get; }

        public ClassResult(string path, CodecTypeInfo classInfo)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            Properties = classInfo.Children!.ToDictionary(x => x.Name!, x => x.Build(path));
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

        public string GetClassHash()
        {
            var props = string.Join("; ", Properties.Select(x => $"{x.Key}:={(x.Value is ClassResult cr ? cr.GetClassHash() : x.Value.ToCSharp())}"));
            var str = $"{ClassName}{(Extending is not null ? $" : {Extending}" : "")}{(IsAbstract ? " abstract" : "")}{{{props}}};";

            using var sha = SHA256.Create();
            var hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(str));

            return HexConverter.ToHex(hashed);
        }
    }
}

