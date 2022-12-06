using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     Represents a scalar result returned from a query.
    /// </summary>
    internal class ScalarResult : IQueryResult
    {
        /// <inheritdoc/>
        public string FileName { get; }

        /// <inheritdoc/>
        public string FilePath { get; }

        /// <summary>
        ///     Gets or sets the dotnet type name.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        ///     Constructs a new <see cref="ScalarResult"/>.
        /// </summary>
        /// <param name="path">The file path of the edgeql query that returns this result.</param>
        /// <param name="info">The codec info used to construct this result.</param>
        public ScalarResult(string path, CodecTypeInfo info)
            : this(path, info.TypeName!) { }

        /// <summary>
        ///     Constructs a new <see cref="ScalarResult"/>.
        /// </summary>
        /// <param name="path">The file path of the edgeql query that returns this result.</param>
        /// <param name="tname">The dotnet type name of the scalar</param>
        public ScalarResult(string path, string tname)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);
            TypeName = tname;
        }

        /// <inheritdoc/>
        public void Visit(ResultVisitor visitor) { }

        /// <inheritdoc/>
        public string ToCSharp()
        {
            return TypeName;
        }
    }
}

