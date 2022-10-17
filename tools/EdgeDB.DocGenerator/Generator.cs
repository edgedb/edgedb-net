using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.DocGenerator
{
    internal class Generator
    {
        private readonly string _outputDir;
        private readonly DocMember[] _docs;

        public Generator(string outputDir, DocMember[] docs)
        {
            _outputDir = outputDir;
            _docs = docs;
        }
        
        public void Generate()
        {
            GenerateExceptionPage();
            GenerateAPIPage();
        }

        private void GenerateExceptionPage()
        {
            var builder = new RSTWriter();

            builder.AppendLine(".. _edgedb-dotnet-exceptions:\n");

            builder.AppendLine("==========");
            builder.AppendLine("Exceptions");
            builder.AppendLine("==========\n");

            foreach(var member in _docs.Where(x => x is DocType dt && dt.DotnetType!.IsAssignableTo(typeof(Exception))).Cast<DocType>().ToArray())
            {
                GenerateTypeSection(member, builder, true);
            }

            var content = builder.ToString();

            File.WriteAllText(Path.Combine(_outputDir, "exceptions.rst"), content);
        }

        private void GenerateAPIPage()
        {
            var builder = new RSTWriter();

            builder.AppendLine(".. _edgedb-dotnet-api:\n");

            builder.AppendLine("=================");
            builder.AppendLine("API Documentation");
            builder.AppendLine("=================\n");

            var namespaces = _docs
                .Where(x => x is DocType dt && !dt.DotnetType!.IsAssignableTo(typeof(Exception)))
                .Cast<DocType>()
                .GroupBy(x => x.DotnetType!.Namespace)
                .ToArray();

            builder.AppendLine("**Namespaces**");
            builder.AppendLine();
            
            foreach (var ns in namespaces)
            {
                builder.AppendLine($"- :dn:namespace:`{ns.Key}`");
            }

            builder.AppendLine();

            foreach (var namespc in namespaces)
            {
                using (_ = builder.BeginScope($".. dn:namespace:: {namespc.Key}"))
                {
                    builder.AppendLine();
                    foreach(var member in namespc)
                    {
                        GenerateTypeSection(member, builder, false);
                    }
                }
            }

            var content = builder.ToString();

            File.WriteAllText(Path.Combine(_outputDir, "api.rst"), content);
        }

        private string GenerateTypeSection(DocType type, RSTWriter builder, bool includeNamespace)
        {
            if (type.DotnetType is null)
                throw new NullReferenceException();

            var tname = GetTypeName(type.DotnetType!).ToLower();

            var typeDefName = includeNamespace
                ? $"{(type.DotnetType is not null ? $"{type.DotnetType?.Namespace}." : "")}{type.Name}"
                : type.Name!;

            // generics
            typeDefName = Regex.Replace(typeDefName, @"`\d+", m =>
            {
                return $"<{string.Join(", ", type.DotnetType!.GetGenericArguments().Select(x => x.Name))}>";
            });

            using (_ = builder.BeginScope($".. dn:{tname}:: {typeDefName}"))
            {
                builder.AppendLine(); // no attributes

                // main description/summary of the type
                FormatInlineDocs(type, builder);

                builder.AppendLine();

                // properties
                foreach (var prop in type.Properties.Where(x => x.PropertyInfo?.GetMethod?.IsPublic ?? false))
                {
                    using (_ = builder.BeginScope($":property {$"{(prop.PropertyInfo is not null ? $"{prop.PropertyInfo.PropertyType.ToFormattedString()} " : "")}{prop.Name}"}:"))
                        FormatInlineDocs(prop, builder);

                    builder.AppendLine();
                }

                // ctors
                foreach (var ctor in type.Constructors.Where(x => x.Method?.IsPublic ?? false))
                {
                    if (ctor.Method is null)
                        throw new Exception("method is null");

                    GenerateMethodSection(ctor, builder);
                }

                // methods
                foreach (var ctor in type.Methods.Where(x => x.Method?.IsPublic ?? false))
                {
                    if (ctor.Method is null)
                        throw new Exception("method is null");

                    GenerateMethodSection(ctor, builder);
                }
            }

            return builder.ToString();
        }

        private void GenerateMethodSection(DocMethod method, RSTWriter builder)
        {
            var declaration = method.Method!.GetSignature(includeParents: false);

            using (_ = builder.BeginScope($".. dn:method:: {declaration}"))
            {
                builder.AppendLine();

                FormatInlineDocs(method, builder);
            }
        }
        
        private void FormatInlineDocs(DocMember member, RSTWriter builder)
        {
            // special case for inheritdoc
            if (member.InlineDocItems.Any(x => x is docMemberInheritDoc) && member is DocMethod method)
            {
                var methodParams = method.Method!.GetParameters();

                bool ScanType(Type type)
                {
                    var m = type.GetMethods().FirstOrDefault(x =>
                    {
                        var p = x.GetParameters();

                        if (p.Length != methodParams!.Length)
                            return false;

                        for (int i = 0; i != p.Length; i++)
                        {
                            var p1 = p[i];
                            var p2 = methodParams[i];

                            if (p1.Name != p2.Name)
                                return false;

                            if (p1.ParameterType != p2.ParameterType)
                                return false;
                        }

                        return x.Name == method.Method.Name;
                    });

                    if (m is not null)
                    {
                        var targetMember = _docs.FirstOrDefault(x => x is DocMethod targetMethod && targetMethod.Parent?.DotnetType == type && targetMethod.Method!.GetSignature() == m.GetSignature());

                        if (targetMember == null)
                        {
                            // pass thru incase of system inherit docs
                            return true;
                        }    

                        FormatInlineDocs(targetMember, builder);
                        return true;
                    }

                    return false;
                }

                foreach(var iface in method.Parent!.DotnetType!.GetInterfaces())
                {
                    if (ScanType(iface))
                        return;
                }

                var btype = method.Parent.DotnetType!.BaseType;

                while(btype is not null)
                {
                    if (ScanType(btype))
                        return;

                    btype = btype.BaseType;
                }

                Console.WriteLine($"Could not find inheritdoc target for {method.Method.GetSignature()} - skipping");
            }

            foreach(var item in member.InlineDocItems.OrderByDescending(x =>
            {
                return x switch
                {
                    docMemberRemarks => 5,
                    docMemberParam => 4,
                    docMemberTypeparam => 3,
                    docMemberReturns => 2,
                    docMemberException => 1,
                    docMemberSummary => 6,
                    _ => 0
                };
            }))
            {
                bool dontAddNewline = false;
                var compiledSummary = item is docMemberSummary sum
                    ? CompileSummary(sum)
                    : Optional<string>.Unspecified;

                if(compiledSummary.IsSpecified && string.IsNullOrEmpty(compiledSummary.Value))
                    continue;

                switch (item)
                {
                    case docMemberException exception:
                        {
                            using (_ = builder.BeginScope($":throws {exception.cref![2..]}:"))
                                builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        }
                        break;
                    case docMemberParam param when member is DocMethod docMethod:
                        {
                            var p = docMethod.Method!.GetParameters().FirstOrDefault(x => x.Name == param.name);

                            if (p is null)
                                throw new Exception($"Failed to find parameter {param.name} on method {docMethod.Name}");

                            using(_ = builder.BeginScope($":param {p.ParameterType.ToFormattedString()} {param.name}:"))
                                builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        }
                        break;
                    case docMemberRemarks remarks:
                        using(_ = builder.BeginScope(".. note::"))
                        {
                            builder.AppendLine();
                            builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        }
                        break;
                    case docMemberReturns returns:
                        using(_ = builder.BeginScope(":returns:"))
                        {
                            builder.AppendLine();
                            builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        }
                        break;
                    case docMemberTypeparam tParam:
                        {
                            using (_ = builder.BeginScope($":param {tParam.name}:"))
                                builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        }
                        break;
                    case docMemberSummary summary:
                        builder.AppendFormattedMultiLine(compiledSummary.Value!);
                        break;
                    default:
                        dontAddNewline = true;
                        break;
                }

                if(!dontAddNewline)
                    builder.AppendLine();
            }
        }

        private string FormatCREF(string cref)
        {
            // check if its def is in the reference and we can reference it via directives
            var type = DocMember.GetTypeOfDefName(cref);
            var nodeName = cref[2..];

            var member = _docs.FirstOrDefault(x => x.Type == type && x.NodeName == nodeName);

            var targetType = type switch
            {
                MemberType.Type => "class",
                MemberType.Method => "method",
                _ => null
            };

            if (member is null || targetType is null)
                return $"``{nodeName}``";

            // fix method sig
            if (nodeName.Contains("(") && member is DocMethod docMethod)
            {
                var sig = docMethod.Method!.GetSignature();

                if(!sig.Contains("()"))
                {
                    var args = sig.Split("(")[1][..1];

                    var newArgs = string.Join(", ", args.Split(", ").Select(x => x.Split(' ')[0]));

                    sig = sig.Replace(args, newArgs);
                }

                nodeName = sig;
            }

            if(member is DocType tp)
            {
                nodeName = Regex.Replace(nodeName, @"`\d+", m =>
                {
                    return $"<{string.Join(", ", tp.DotnetType!.GetGenericArguments().Select(x => x.Name))}>";
                });
            }

            return $":dn:{targetType}:`{nodeName}`";
        }

        private string CompileSummary(docMemberSummary summary)
        {
            List<string> compiledContent = new();

            if (summary.Text is null)
                return string.Empty;
            
            for(int i = 0; i != summary.Text.Length; i++)
            {
                var text = summary.Text[i];

                text = Regex.Replace(text, @"\s+", " ", RegexOptions.Multiline);

                if(text.StartsWith('\n') || i == 0)
                    text = text.TrimStart();

                if (i != 0 && summary.Items is not null && summary.ItemsElementName is not null && summary.Items.Length >= i)
                {
                    string? content = null;

                    // pull the item and parse it
                    var itemType = summary.ItemsElementName[i - 1];
                    var item = summary.Items[i - 1];
                    
                    switch (itemType)
                    {
                        case ItemsChoiceType.see when item is docMemberSummarySee see:
                            {
                                if(see.href is not null)
                                {
                                    if (string.IsNullOrEmpty(see.Value))
                                        content = see.href;
                                    else
                                        content = $"`{see.Value} <{see.href}>`_";
                                }
                                else if (see.langword is not null)
                                {
                                    content = $"``{see.langword}``";
                                }
                                else if (see.cref is not null)
                                {
                                    content = FormatCREF(see.cref);
                                }
                            }
                            break;
                        case ItemsChoiceType.typeparamref when item is docMemberSummaryTypeparamref tref:
                            content = $"``{tref.name}``";
                            break;
                        case ItemsChoiceType.i:
                            content = $"*{item}*";
                            break;
                        case ItemsChoiceType.c:
                            content = $"``{item}``";
                            break;
                        case ItemsChoiceType.paramref when item is docMemberSummaryParamref pref:
                            content = $"``{pref}``";
                            break;
                        case ItemsChoiceType.br:
                            content = "\n";
                            break;
                    }

                    if (content is not null)
                        compiledContent.Add(content);
                    else
                        Console.WriteLine($"Warning: no content generated for {itemType}:{item}");

                    compiledContent.Add(text);
                }
                else
                    compiledContent.Add(text);
            }

            string result = compiledContent.Any() ? compiledContent[0] : string.Empty;

            for(int i = 1; i < compiledContent.Count; i++)
            {
                var last = compiledContent[i - 1];
                var cur = compiledContent[i];

                if (Regex.IsMatch(last, @"\w$") && Regex.IsMatch(cur, @"^\w"))
                {
                    result += $" {cur}";
                }
                else if (Regex.IsMatch(last, @"\w$") && cur[0] is not ':' or '`')
                    result += cur;
                else if (!last.EndsWith(' ') && Regex.IsMatch(cur, @"^\w"))
                    result += $" {cur}";
                else if (cur[0] is ':' or '`' or '*')
                    result += $"{(last.Last() == ' ' ? "" : " ")}{cur}";
                else
                    result += cur;
            }

            return Regex.Replace(result, @"\n (\w)", m =>
            {
                return $"\n{m.Groups[1].Value}";
            }, RegexOptions.Multiline);
        }
        

        private string GetTypeName(Type type)
        {
            if (type.IsClass)
                return "Class";
            else if (type.IsInterface)
                return "Interface";
            else if (type.IsValueType)
                return "Struct";

            return "Type";
        }
    }
}
