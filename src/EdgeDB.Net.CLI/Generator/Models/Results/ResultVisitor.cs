using System;
namespace EdgeDB.CLI.Generator.Results
{
    internal class ResultVisitor
    {
        public List<ClassResult> GenerationTargets { get; } = new();

        public void AddTypeGenerationTarget(ClassResult definition)
        {
            if(!GenerationTargets.Contains(definition))
                GenerationTargets.Add(definition);
        }
    }
}

