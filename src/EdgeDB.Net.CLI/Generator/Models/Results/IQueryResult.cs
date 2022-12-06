using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal interface IQueryResult
    {
        /// <summary>
        ///     Gets the file name (without extensions) that returns this result.
        /// </summary>
        string FileName { get; }

        /// <summary>
        ///     Gets the full path of the edgeql file that returns this result.
        /// </summary>
        string FilePath { get; }

        void Visit(ResultVisitor visitor);
        string ToCSharp();
    }
}

