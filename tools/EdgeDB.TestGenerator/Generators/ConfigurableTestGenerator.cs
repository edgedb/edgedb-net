using System;
namespace EdgeDB.TestGenerator.Generators
{
    internal class ConfigurableTestGenerator : TestGenerator
    {
        private readonly RuleSetConfiguration _config;
        private readonly GroupDefinition _group;
        private readonly GenerationRuleSet _rules;

        public ConfigurableTestGenerator(GroupDefinition group, RuleSetConfiguration config)
        {
            _config = config;
            _group = group;
            _rules = config.ToRuleSet();
        }

        protected override QueryDefinition GetQuery(ValueGenerator.GenerationResult result)
            => new QueryDefinition(
                   _config.QueryTemplate!.FormatQuery(result.FormatVariableIdentifier)!,
                   _config.QueryTemplate.FormatArguments(result.FormatVariableIdentifier)
               );

        protected override TestGroup GetTestGroup()
            => GetOrAddTestGroup(_group);

        protected override string GetTestName(ValueGenerator.GenerationResult result)
            => _config.QueryTemplate!.FormatName(result.FormatVariableIdentifier)!;

        protected override GenerationRuleSet GetTestSetRules()
            => _rules;
    }
}

