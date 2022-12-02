using EdgeDB.CLI.Generator.Models;
using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal class ScalarResult : IQueryResult
    {
        private readonly CodecTypeInfo _info;

        public ScalarResult(CodecTypeInfo info)
        {
            _info = info;
        }

        public void Visit(ResultVisitor visitor) { }

        public string ToCSharp()
        {
            return _info.TypeName!;
        }
    }
}

