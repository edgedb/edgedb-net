using System.Collections.Immutable;

namespace EdgeDB.State;

internal sealed class Session
{
    private Dictionary<string, string> _aliases = new();
    private Config _config;

    private Dictionary<string, object?> _globals = new();

    public Session()
    {
        Module ??= "default";
        _config ??= new Config();
    }

    public string Module { get; init; }

    public IReadOnlyDictionary<string, string> Aliases
    {
        get => _aliases.ToImmutableDictionary();
        init => _aliases = value.ToDictionary(x => x.Key, x => x.Value);
    }

    public Config Config { get => _config; init => _config = value; }

    public IReadOnlyDictionary<string, object?> Globals
    {
        get => _globals.ToImmutableDictionary();
        init => _globals = value.ToDictionary(x => x.Key, x => x.Value);
    }

    public static Session Default
        => new();

    internal IDictionary<string, object?> Serialize()
    {
        var dict = new Dictionary<string, object?>();
        if (Module != "default")
            dict["module"] = Module;

        if (_aliases.Any())
            dict["aliases"] = Aliases;

        var serializedConfig = Config.Serialize();
        if (serializedConfig.Any())
            dict["config"] = serializedConfig;

        if (_globals.Any())
        {
            dict["globals"] = Globals.Select(x =>
                x.Key.Contains("::")
                    ? (x.Key, x.Value)
                    : ($"{Module}::{x.Key}", x.Value)
            ).ToDictionary(x => x.Item1, x => x.Value);
        }

        return dict;
    }

    public Session WithGlobals(IDictionary<string, object?> globals) =>
        new()
        {
            _globals = globals.ToDictionary(x => x.Key, x => x.Value),
            _aliases = _aliases,
            _config = _config,
            Module = Module
        };

    public Session WithModuleAliases(IDictionary<string, string> aliases) =>
        new()
        {
            _aliases = aliases.ToDictionary(x => x.Key, x => x.Value),
            _config = _config,
            _globals = _globals,
            Module = Module
        };

    public Session WithConfig(Config config) =>
        new() {_config = config, _aliases = _aliases, _globals = _globals, Module = Module};

    public Session WithModule(string module) =>
        new() {_config = _config, _aliases = _aliases, _globals = _globals, Module = module};
}
