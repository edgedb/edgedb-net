using System;
namespace EdgeDB.CLI.Generator.Models
{
    /// <summary>
    ///     A class representing the result of a edgeql -> cs generation operation.
    /// </summary>
    public class GenerationResult
    {
        /// <summary>
        ///     Gets the code generated from edgeql.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        ///     Gets the hash of the edgeql and header of the generated code.
        /// </summary>
        public string? EdgeQLHash { get; set; }

        /// <summary>
        ///     Gets the name of the class containing the execute method.
        /// </summary>
        public string? ExecuterClassName { get; set; }

        /// <summary>
        ///     Gets the name of the return result that the executer method returns.
        /// </summary>
        public string? ReturnResult { get; set; }

        /// <summary>
        ///     Gets a collection of parameters (edgeql arguments) for the executer function.
        /// </summary>
        public IEnumerable<string>? Parameters { get; set; }

        public List<string> GeneratedTypeFiles { get; set; } = new();
    }
}

