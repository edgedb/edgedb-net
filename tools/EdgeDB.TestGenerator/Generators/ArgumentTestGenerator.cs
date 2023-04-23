using EdgeDB.TestGenerator.ValueProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Generators
{
    internal class ArgumentTestGenerator : TestGenerator
    {
        protected override QueryDefinition GetQuery(ValueGenerator.GenerationResult result)
            => new($"select <{result.EdgeDBTypeName}>$arg", new Dictionary<string, object?>
            {
                { "arg", result.Value }
            }, Cardinality.One);

        protected override TestGroup GetTestGroup()
            => TestGenerator.ArgumentTestGroup;

        protected override string GetTestName(ValueGenerator.GenerationResult result)
            => $"Argument of type {result.EdgeDBTypeName}";

        protected override ValueGenerator.GenerationRuleSet GetTestSetRules()
            => ValueGenerator.V2ArugmentRuleSet;
    }
}
