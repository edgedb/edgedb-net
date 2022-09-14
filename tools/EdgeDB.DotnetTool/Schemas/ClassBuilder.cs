using EdgeDB.Binary;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EdgeDB.DotnetTool
{
    internal class ClassBuilder
    {
        private readonly string _outputDir;
        private readonly string _generatedNamespace;

        private readonly TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;


        public ClassBuilder(string outputDir, string generatedNamespace)
        {
            _outputDir = outputDir;
            _generatedNamespace = generatedNamespace;
        }

        public void Generate(Module module, Func<string, string> nameCallback)
        {
            var context = new ClassBuilderContext
            {
                Module = module,
                NameCallback = nameCallback
            };

            // create the module folder
            var folder = Path.Combine(_outputDir, _textInfo.ToTitleCase(module.Name!));
            Directory.CreateDirectory(folder);

            context.OutputDir = folder;

            foreach (var type in module.Types)
            {
                if (context.BuiltTypes.Contains(type))
                    continue;

                context.Type = type;
                GenerateType(type, folder, context);

                context.BuiltTypes.Add(type);
            }
        }

        public void GenerateType(Type t, string dir, ClassBuilderContext context)
        {
            var writer = new CodeWriter();

            writer.AppendLine($"// Generated on {DateTimeOffset.UtcNow:O}");
            writer.AppendLine("using EdgeDB;");

            string? name = t.Name;

            if (name != null && Regex.IsMatch(name, @"^[`'](.*?)[`']$"))
                name = Regex.Match(name, @"^[`'](.*?)[`']$").Groups[1].Value;

            using (var _ = writer.BeginScope($"namespace {_generatedNamespace}"))
            {
                name = !IsValidDotnetName(name)
                    ? context.NameCallback($"{context.Module?.Name}::{name ?? "null"}")
                    : PascalUtils.ToPascalCase(name);

                t.BuiltName = name;

                if (IsEnum(t))
                {
                    writer.AppendLine("[EnumSerializer(SerializationMethod.Lower)]");
                    using (var __ = writer.BeginScope($"public enum {name}"))
                    {
                        // extract values
                        foreach (Match match in Regex.Matches(t.Extending!, @"(?><| )(.*?)(?>,|>)"))
                        {
                            writer.AppendLine($"{match.Groups[1].Value},");
                        }
                    }
                }
                else
                {
                    // generate class
                    writer.AppendLine($"[EdgeDBType(\"{t.Name}\")]");
                    using (var __ = writer.BeginScope($"public{(t.IsAbstract ? " abstract" : "")} class {name} : {ResolveTypename(context!, t.Extending) ?? "BaseObject"}"))
                    {
                        foreach (var prop in t.Properties)
                        {
                            if (context.BuildProperties.Contains(prop))
                                continue;

                            context.Property = prop;
                            GenerateProperty(writer, prop, context);

                            context.BuildProperties.Add(prop);
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(dir, $"{name}.g.cs"), writer.ToString());
            Console.WriteLine($"Wrote {name}.g.cs : {Path.Combine(dir, $"{name}.g.cs")}");
        }

        private string? GenerateProperty(CodeWriter writer, Property prop, ClassBuilderContext context)
        {
            // TODO: build a tree of contraints to use when applying attributes to props.
            if (prop.IsStrictlyConstraint)
                return null;

            // get the C# type if its a std type
            var resolved = ResolveTypename(context, prop.Type);
            var type = ResolveScalar(resolved)?.FullName ?? resolved;

            // convert to pascal case for name
            var name = prop.Name;

            name = !IsValidDotnetName(name)
                ? context.NameCallback($"{context.Module?.Name}::{context.Type?.Name}.{name ?? "null"}")
                : PascalUtils.ToPascalCase(name);

            prop.BuiltName = name;

            if (type == null && prop.IsComputed && prop.ComputedValue != null)
            {
                var computed = prop.ComputedValue;

                var wrappedMatch = Regex.Match(computed, @"^\((.*?)\)$");

                if (wrappedMatch.Success)
                    computed = wrappedMatch.Groups[1].Value;

                // check for backlink
                if (computed.StartsWith(".<"))
                {
                    // set the cardinaliry to multi since its a backlinl
                    prop.Cardinality = PropertyCardinality.Multi;

                    var match = Regex.Match(computed, @"\[is (.+?)\]");

                    type = match.Success ? ResolveTypename(context, match.Groups[1].Value) : "BaseObject";
                }
                else
                {
                    // do a reverse lookup on the root function to see if we can decipher the type
                    computed = Regex.Replace(computed, @"^.+?::", _ => "");
                    var returnType = typeof(object); //QueryBuilder.ReverseLookupFunction(computed);

                    if (returnType != null)
                        type = returnType.FullName;
                    else if (computed.StartsWith("."))
                    {
                        // its a prop ref, generate it
                        var pName = Regex.Match(computed, @"^\.(\w+)").Groups[1].Value;
                        var p = context.Type?.Properties.FirstOrDefault(x => x.Name == pName)!;

                        if (context.BuildProperties.Any(x => x.Name == p.Name))
                            type = context.BuildProperties.FirstOrDefault(x => x.Name == p.Name)!.Type;
                        else
                        {
                            type = GenerateProperty(writer, p, context);
                            context.BuildProperties.Add(p);
                        }
                    }
                    else
                    {
                        type = "object";
                    }
                }
            }

            if (prop.Cardinality == PropertyCardinality.Multi)
                type = $"Set<{type}>";

            writer.AppendLine($"[EdgeDBProperty(\"{prop.Name}\", IsLink = {Lower(prop.IsLink)}, IsRequired = {Lower(prop.Required)}, IsReadOnly = {Lower(prop.ReadOnly)}, IsComputed = {Lower(prop.IsComputed)})]");

            // TODO: maybe remove set operator for readonly / computed?
            writer.AppendLine($"public {type} {name} {{ get; set; }}");

            prop.Type = type;

            return type;

        }

        public static System.Type? ResolveScalar(string? t)
            => t == null
                ? null
                : PacketSerializer.GetDotnetType(Regex.Replace(t, @".+?::", m => ""));

        private string? ResolveTypename(ClassBuilderContext context, string? name)
        {
            if (name == null)
                return null;

            if (name.StartsWith($"{context.Module!.Name}::"))
                name = name[(context.Module.Name!.Length + 2)..];

            // check our built types
            if (context.BuiltTypes.Any(x => x.Name == name))
            {
                return context.BuiltTypes.FirstOrDefault(x => x.Name == name)!.BuiltName;
            }
            else if (context.Module.Types.Any(x => x.Name == name)) // try building it if it has a name ref
            {
                var type = context.Module.Types.FirstOrDefault(x => x.Name == name)!;
                GenerateType(type, context.OutputDir!, context);
                context.BuiltTypes.Add(type);
                return type.BuiltName;
            }

            return name;
        }

        internal static bool IsValidDotnetName(string? name)
        {
            if (name == null)
                return false;

            return Regex.IsMatch(name, @"^[a-zA-Z@_](?>\w|@)+?$");
        }

        private static string Lower(bool val)
            => val.ToString().ToLower();
        private static bool IsEnum(Type t)
            => t.IsScalar && (t.Extending?.StartsWith("enum") ?? false);
    }
}
