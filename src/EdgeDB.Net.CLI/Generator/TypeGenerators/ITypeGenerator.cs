using EdgeDB.Binary.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Generator.TypeGenerators
{
    internal interface ITypeGenerator<TContext> : ITypeGenerator
        where TContext : ITypeGeneratorContext
    {
        new TContext CreateContext(GeneratorContext generatorContext);

        /// <summary>
        ///     Parses the information within the codec to return out a string representing c# code
        ///     that can be used in the main generated file.
        /// </summary>
        /// <remarks>
        ///     For example, when ICodec is a string codec, this function should return "<c>string</c>".
        ///     If the codec is an object codec, it should generate a class/struct that represents the
        ///     given codec, and return its name.
        /// </remarks>
        /// <param name="codec">The codec to derive the type from.</param>
        /// <param name="target">The target information if this is for the result of the target; othewise <see langword="null"/>.</param>
        /// <param name="context">The context of this generator</param>
        /// <returns></returns>
        ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, TContext context);

        ValueTask<string> ITypeGenerator.GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, ITypeGeneratorContext context)
            => context is TContext tc
                ? GetTypeAsync(codec, target, tc)
                : throw new InvalidCastException("Mismatched context");

        ITypeGeneratorContext ITypeGenerator.CreateContext(GeneratorContext generatorContext)
            => CreateContext(generatorContext);
    }

    internal interface ITypeGenerator
    {
        ITypeGeneratorContext CreateContext(GeneratorContext generatorContext);

        ValueTask<string> GetTypeAsync(ICodec codec, GeneratorTargetInfo? target, ITypeGeneratorContext context);
    }
}
