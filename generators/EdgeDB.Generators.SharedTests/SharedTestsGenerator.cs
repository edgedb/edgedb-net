using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EdgeDB.Generators.SharedTests
{
    [Generator]
    public class SharedTestsGenerator : ISourceGenerator
    {
        private readonly Regex _groupRegex = new Regex("v(\\d+)_(.+)\\.json");
        public void Execute(GeneratorExecutionContext context)
        {
            var groupFiles = context.AdditionalFiles.Where(x => _groupRegex.IsMatch(Path.GetFileName(x.Path)));

            foreach(var groupFile in groupFiles)
            {
                var testFiles = Directory.GetFiles(
                    Path.Combine(
                        Path.GetDirectoryName(groupFile.Path),
                        Path.GetFileNameWithoutExtension(groupFile.Path)
                    )
                );

                WriteTestGroup(
                    context,
                    JsonConvert.DeserializeObject<TestGroup>(groupFile.GetText()!.ToString())!,
                    testFiles
                );
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }

        private void WriteTestGroup(GeneratorExecutionContext context, TestGroup group, IEnumerable<string> testFiles)
        {
            var writer = new CodeWriter();

            writer.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            writer.AppendLine("using System.Threading.Tasks;");
            writer.AppendLine();

            var groupName = ToPascal(group.Name!);
            using (var nsScope = writer.BeginScope("namespace EdgeDB.Tests.Integration.SharedTests"))
            {
                writer.AppendLine("[TestClass]");
                using (var clsScope = writer.BeginScope($"public class {groupName}"))
                {
                    foreach (var testFile in testFiles)
                    {
                        var line = File.ReadLines(testFile).Skip(1).Take(1).First();

                        var test = JsonConvert.DeserializeObject<Test>($"{{ {line} }}");

                        writer.AppendLine($"[TestMethod(\"{test!.Name}\")]");
                        using (var mthScope = writer.BeginScope($"public Task {groupName}_{Path.GetFileNameWithoutExtension(testFile)}()"))
                        {
                            writer.AppendLine($"var path = @\"{testFile}\";");
                            writer.AppendLine("return SharedTestsRunner.RunAsync(path);");
                        }

                        writer.AppendLine();
                    }
                }
            }

            context.AddSource(groupName, writer.ToString());
        }


        private readonly Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
        private readonly Regex whiteSpace = new Regex(@"(?<=\s)");
        private readonly Regex startsWithLowerCaseChar = new Regex("^[a-z]");
        private readonly Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
        private readonly Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
        private readonly Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");
        private string ToPascal(string str)
        {
            // replace white spaces with undescore, then replace all invalid chars with empty string
            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(str, "_"), string.Empty)
                // split by underscores
                .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                // set first letter to uppercase
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }
    }

    internal class TestGroup
    {
        public string? ProtocolVersion { get; set; }
        public string? Name { get; set; }
    }

    internal class Test
    {
        public string? Name { get; set; }
    }
}
