using EdgeDB.State;
using System;
using System.Text;

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

        protected override IEnumerable<QueryDefinition> GetQueries(ValueGenerator.GenerationResult result)
            => _config.QueryTemplates!.Select(x =>
            {
                return new QueryDefinition(
                    x!.FormatName(_rules, result.FormatVariableIdentifier)!,
                    x.FormatQuery(_rules, result.FormatVariableIdentifier)!,
                    x.FormatArguments(_rules, result.FormatVariableIdentifier),
                    Cardinality: x.Cardinality,
                    Capabilities: x.Capabilities,
                    ConfigureState: (c) => ConfigureState(c, x, result)
                );
            });

        protected override TestGroup GetTestGroup()
            => GetOrAddTestGroup(_group);

        protected override GenerationRuleSet GetTestSetRules()
            => _rules;

        protected override string GetTestName(ValueGenerator.GenerationResult result)
            => FormatterUtils.Format(_rules, _config.TestNameTemplate!, result.FormatVariableIdentifier);

        private BaseEdgeDBClient ConfigureState(
            BaseEdgeDBClient client,
            QueryTemplate template,
            ValueGenerator.GenerationResult result)
        {
            var session = new Session
            {
                Module = template.Session?.Module ?? "default",
                Aliases = template.Session?.Aliases ?? new Dictionary<string, string>(),
                Config = template.Session?.Config is not null
                    ? new Config()
                    {
                        AllowDMLInFunctions = template.Session.Config.AllowDMLInFunctions,
                        ApplyAccessPolicies = template.Session.Config.ApplyAccessPolicies,
                        DDLPolicy = template.Session.Config.DDLPolicy,
                        IdleTransationTimeout = template.Session.Config.IdleTransationTimeout,
                        QueryExecutionTimeout = template.Session.Config.QueryExecutionTimeout,
                    }
                    : Config.Default,
                Globals = FormatterUtils.FormatKVP(_rules, result.FormatVariableIdentifier, template.Session?.Globals, result) ?? new Dictionary<string, object?>()
            };

            return client.WithSession(session);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(_group.Name);

            if (_rules.Name is not null)
                sb.Append($": {_rules.Name}");

            return sb.ToString();
        }
    }
}

