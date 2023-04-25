using EdgeDB.TestGenerator.ValueProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.TestGenerator.Generators
{
    internal class QueryResultTestGenerator : TestGenerator
    {
        protected override QueryDefinition GetQuery(ValueGenerator.GenerationResult result)
            => new($"select {result.ToEdgeQLFormat()}");

        protected override TestGroup GetTestGroup()
            => TestGenerator.QueryTestGroup;

        protected override string GetTestName(ValueGenerator.GenerationResult result)
            => $"Query result of type {result.EdgeDBTypeName}";

        protected override GenerationRuleSet GetTestSetRules()
            => ValueGenerator.QueryResultRuleSet;
    }
}
