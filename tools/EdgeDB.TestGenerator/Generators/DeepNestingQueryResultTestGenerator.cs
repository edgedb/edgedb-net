using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Generators
{
    internal class DeepNestingQueryResultTestGenerator : TestGenerator
    {
        protected override QueryDefinition GetQuery(ValueGenerator.GenerationResult result)
           => new($"select {result.ToEdgeQLFormat()}");

        protected override TestGroup GetTestGroup()
            => TestGenerator.DeepNestingTestGroup;

        protected override string GetTestName(ValueGenerator.GenerationResult result)
            => $"Deep nesting query result of type {result.EdgeDBTypeName}";

        protected override GenerationRuleSet GetTestSetRules()
            => ValueGenerator.DeepQueryResultNesting;
    }
}
