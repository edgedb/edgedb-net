using EdgeDB.CLI.Generator.Models;
using System;
using System.IO;

namespace EdgeDB.CLI.Generator.Results
{
    internal class ScalarResult : IQueryResult
    {
        public string FileName { get; }

        public string FilePath { get; }

        public string TypeName { get; set; }

        public ScalarResult(string path, CodecTypeInfo info)
            : this(path, info.TypeName!) { }

        public ScalarResult(string path, string tname)
        {
            FilePath = Path.GetFullPath(path);
            FileName = Path.GetFileNameWithoutExtension(path);
            TypeName = tname;
        }

        public void Visit(ResultVisitor visitor) { }

        public string ToCSharp()
        {
            return TypeName;
        }
    }
}

