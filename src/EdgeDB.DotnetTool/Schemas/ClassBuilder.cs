using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal class ClassBuilder
    {
        private readonly string _outputDir;
        private readonly string _generatedNamespace;

        private readonly TextInfo _textInfo = new CultureInfo("en-US",false).TextInfo;


        public ClassBuilder(string outputDir, string generatedNamespace)
        {
            _outputDir = outputDir;
            _generatedNamespace = generatedNamespace;
        }

        public void Generate(Module module)
        {
            // create the module folder
            var folder = Path.Combine(_outputDir, _textInfo.ToTitleCase(module.Name!));
            Directory.CreateDirectory(folder);

            foreach(var type in module.Types)
            {
                GenerateType(type, module, folder);
            }
        }

        public void GenerateType(Type t, Module m, string dir)
        {
            var writer = new CodeWriter();

            writer.AppendLine($"// Generated on {DateTimeOffset.UtcNow.ToString("O")}");
            writer.AppendLine("using EdgeDB;");

            using(var _ = writer.BeginScope($"namespace {_generatedNamespace}"))
            {
                if (IsEnum(t))
                {
                    writer.AppendLine("[EnumSerializer(SerializationMethod.Lower)]");
                    using(var __ = writer.BeginScope($"public enum {t.Name}"))
                    {
                        // extract values
                        foreach(Match match in Regex.Matches(t.Extending!, @"(?><| )(.*?)(?>,|>)"))
                        {
                            writer.AppendLine($"{match.Groups[1].Value},");
                        }
                    }
                }
                else
                {
                    // generate class
                    writer.AppendLine($"[EdgeDBType(\"{t.Name}\")]");
                    using(var __ = writer.BeginScope($"public{(t.IsAbstract ? " abstract" : "")} class {PascalUtils.ToPascalCase(t.Name!)} : {ResolveTypename(m, t.Extending) ?? "BaseObject"}"))
                    {
                        List<Property> writtenProperties = new();
                        foreach(var prop in t.Properties)
                        {
                            if (writtenProperties.Contains(prop))
                                continue;

                            GenerateProperty(writer, prop, t, m, ref writtenProperties);

                            writtenProperties.Add(prop);
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(dir, $"{PascalUtils.ToPascalCase(t.Name!)}.g.cs"), writer.ToString());
            Console.WriteLine($"Wrote {PascalUtils.ToPascalCase(t.Name!)}.g.cs : {Path.Combine(dir, $"{PascalUtils.ToPascalCase(t.Name!)}.g.cs")}");
        }

        private string? GenerateProperty(CodeWriter writer, Property prop, Type t, Module m, ref List<Property> written)
        {
            // get the C# type if its a std type
            var resolved = ResolveTypename(m, prop.Type);
            var type = ResolveScalar(resolved)?.FullName ?? resolved;

            // convert to pascal case for name
            var name = PascalUtils.ToPascalCase(prop.Name!);

            if(type == null && prop.IsComputed && prop.ComputedValue != null)
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

                    if (match.Success)
                        type = ResolveTypename(m, match.Groups[1].Value);
                    else
                        type = "BaseObject";
                }
                else
                {
                    // do a reverse lookup on the root function to see if we can decipher the type
                    computed = Regex.Replace(computed, @"^.+?::", _ => "");
                    var returnType = QueryBuilder.ReverseLookupFunction(computed);

                    if (returnType != null)
                        type = returnType.FullName;
                    else if (computed.StartsWith("."))
                    {
                        // its a prop ref, generate it
                        var pName = Regex.Match(computed, @"^\.(\w+)").Groups[1].Value;
                        var p = t.Properties.FirstOrDefault(x => x.Name == pName)!;

                        if (written.Any(x => x.Name == p.Name))
                            type = written.FirstOrDefault(x => x.Name == p.Name)!.Type;
                        else
                        {
                            type = GenerateProperty(writer, p, t, m, ref written);

                            written.Add(p);
                        }
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

        public System.Type? ResolveScalar(string? t)
        {
            return t == null ? null : PacketSerializer.GetDotnetType(Regex.Replace(t, @".+?::", m => ""));
        }

        private string? ResolveTypename(Module m, string? name)
        {
            if (name == null)
                return null;

            if (name.StartsWith($"{m.Name}::"))
                return name.Substring(m.Name!.Length + 2);

            return name;
        }

        private string Lower(bool val)
            => val.ToString().ToLower();
        private bool IsEnum(Type t)
            => t.IsScalar && (t.Extending?.StartsWith("enum") ?? false);
    }
}
