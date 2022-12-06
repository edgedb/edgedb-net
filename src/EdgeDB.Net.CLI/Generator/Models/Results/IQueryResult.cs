using System;
namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     Represents a query result returned from a query.
    /// </summary>
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

        /// <summary>
        ///     Visits this result and tracks the class types.
        /// </summary>
        /// <param name="visitor">The <see cref="ResultVisitor"/> used to visit this result.</param>
        void Visit(ResultVisitor visitor);

        /// <summary>
        ///     Converts this query result to a C# representation.
        /// </summary>
        /// <returns></returns>
        string ToCSharp();
    }
}

