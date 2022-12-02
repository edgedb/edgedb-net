using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal interface IQueryResult
    {
        void Visit(ResultVisitor visitor);
        string ToCSharp();
    }
}

