using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal sealed class V2TypeGeneratorContext : ITypeGeneratorContext
    {
        public required GeneratorContext GeneratorContext { get; init; }

        public IEnumerable<Task> FileGenerationTasks
            => _work;

        private readonly List<Task> _work = new();

        public void QueueWork(Task task)
            => _work.Add(task);

    }

    internal sealed class V2TypeGenerator : ITypeGenerator<V2TypeGeneratorContext>
    {
        public V2TypeGeneratorContext CreateContext(GeneratorContext generatorContext)
            => new() { GeneratorContext = generatorContext };

        public ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, V2TypeGeneratorContext context)
        {
            return ValueTask.FromResult("object?");
        }
    }
}
