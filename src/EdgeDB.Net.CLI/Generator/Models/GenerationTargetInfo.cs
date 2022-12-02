using System;
namespace EdgeDB.CLI.Generator.Models
{
    /// <summary>
    ///     Represents a generation target, containing useful information for it.
    /// </summary>
    public class GenerationTargetInfo
    {
        /// <summary>
        ///     Gets or sets the edgeql file name without extension.
        /// </summary>
        public string? EdgeQLFileNameWithoutExtension { get; set; }

        /// <summary>
        ///     Gets or sets the edgeql file path.
        /// </summary>
        public string? EdgeQLFilePath { get; set; }

        /// <summary>
        ///     Gets or sets the output target file path.
        /// </summary>
        public string? TargetFilePath { get; set; }

        /// <summary>
        ///     Gets or sets the edgeql.
        /// </summary>
        public string? EdgeQL { get; set; }

        /// <summary>
        ///     Gets or sets the hash of the edgeql.
        /// </summary>
        public string? EdgeQLHash { get; set; }

        /// <summary>
        ///     Gets or sets whether or not this is a
        ///     create operation; else is a update operation.
        /// </summary>
        public bool WasCreated { get; set; }

        /// <summary>
        ///     Checks if the target file exists and the header matches the hash of the edgeql.
        /// </summary>
        /// <returns></returns>
        public bool IsGeneratedTargetExistsAndIsUpToDate()
        {
            var lines = File.Exists(TargetFilePath) ? File.ReadAllLines(TargetFilePath) : Array.Empty<string>();

            return File.Exists(TargetFilePath) && lines.Length >= 2 && CodeGenerator.TargetFileHashMatches(lines[1], EdgeQLHash!);
        }
    }
}

