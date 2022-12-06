using System;
namespace EdgeDB.CLI.Generator.Results
{
    /// <summary>
    ///     A class used to walk the result tree and find all class results.
    /// </summary>
    internal class ResultVisitor
    {
        /// <summary>
        ///     Gets all <see cref="ClassResult"/>s that were found within the tree.
        /// </summary>
        public List<ClassResult> GenerationTargets { get; } = new();

        /// <summary>
        ///     Adds a <see cref="ClassResult"/> as a generation target.
        /// </summary>
        /// <param name="definition">The <see cref="ClassResult"/> to add as a generation target.</param>
        public void AddTypeGenerationTarget(ClassResult definition)
        {
            if(!GenerationTargets.Contains(definition))
                GenerationTargets.Add(definition);
        }
    }
}

