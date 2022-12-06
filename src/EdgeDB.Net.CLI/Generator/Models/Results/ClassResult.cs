using EdgeDB.CLI.Generator.Models;
using EdgeDB.Utils;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     Represents a object result returned from a query, reimagined as a class.
    /// </summary>
    internal class ClassResult : IQueryResult
    {
        /// <inheritdoc/>
        public string FileName { get; }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <summary>
        ///     Gets or sets the class name of this class result.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        ///     Gets or sets the base type of this result.
        /// </summary>
        public string? Extending { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this class is abstract.
        /// </summary>
        public bool IsAbstract { get; set; }

        /// <summary>
        ///     Gets or sets the properties of this class.
        /// </summary>
        public Dictionary<string, IQueryResult> Properties { get; set; }

        /// <summary>
        ///     Gets the used namespaces of this class.
        /// </summary>
        public IEnumerable<string> UsedNamespaces { get; }

        /// <summary>
        ///     Constructs a new <see cref="ClassResult"/>.
        /// </summary>
        /// <param name="path">The file path of the edgeql query that returns this result.</param>
        /// <param name="info">The codec info used to construct this result.</param>
        public ClassResult(string path, CodecTypeInfo info)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);

            Properties = info.Children!.ToDictionary(x => x.Name!, x => x.Build(path));
            ClassName = info.Name ?? info.GetUniqueTypeName();

            UsedNamespaces = GetNamespaces(info).Distinct();
        }

        /// <summary>
        ///     Gets a collection of namespaces used by this class.
        /// </summary>
        /// <param name="typeInfo">The type info of the codec.</param>
        /// <returns>A collection containing the used namespaces.</returns>
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

        /// <inheritdoc/>
        public void Visit(ResultVisitor visitor)
        {
            visitor.AddTypeGenerationTarget(this);

            foreach(var prop in Properties)
            {
                prop.Value.Visit(visitor);
            }
        }

        /// <inheritdoc/>
        public string ToCSharp()
        {
            return ClassName;
        }

        /// <summary>
        ///     Gets a unique, hex encoded string which represents the current <see cref="ClassResult"/>.
        /// </summary>
        /// <returns>A SHA256 hash, encoded as hex.</returns>
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

