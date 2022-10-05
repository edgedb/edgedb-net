using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DocGenerator
{
    internal class Generator
    {
        public static void Generate(string output, DocMember[] doc)
        {
            GenerateExceptionPage(output, doc.Where(x => x is DocType dt && dt.DotnetType!.IsAssignableTo(typeof(Exception))).ToArray());
        }

        public static void GenerateExceptionPage(string output, DocMember[] members)
        {
            var builder = new StringBuilder();

            builder.AppendLine(".. _edgedb-dotnet-exceptions:\n");

            builder.AppendLine("==========");
            builder.AppendLine("Exceptions");
            builder.AppendLine("==========\n");

            foreach(var member in members)
            {
                builder.AppendLine(GenerateTypeSection(member));
                builder.AppendLine();
            }

            var t = builder.ToString();
        }

        private static string GenerateTypeSection(DocMember member)
        {
            var builder = new StringBuilder();

            builder.AppendLine($".. dn:class:: {member.NodeName}\n");

            foreach(var item in member.InlineDocItems!)
            {
                switch (item)
                {
                    case docMemberSummary summary:
                        builder.Append(BuildSummary(summary));
                        break;
                }
            }

            return builder.ToString();
        }

        private static string BuildSummary(docMemberSummary summary)
        {
            var builder = new StringBuilder();
            
            for(int i = 0; i != summary.Text!.Length; i++)
            {
                builder.Append(summary.Text[i].Trim().Replace("\n", ""));

                if(summary.Items?.Length > i)
                {
                    var item = summary.Items[i];

                    switch (item)
                    {
                        case docMemberSummarySee seeHref when !string.IsNullOrEmpty(seeHref.href):
                            {
                                builder.Append($"`{seeHref.Value ?? seeHref.href} <{seeHref.href}>`");
                            }
                            break;
                        case docMemberSummarySee langword when !string.IsNullOrEmpty(langword.langword):
                            {
                                builder.Append($"`{langword.langword} <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/{langword.langword}>`");
                            }
                            break;
                        default:
                            throw new Exception($"Unhandled renderer for item {item}");
                    }
                }
            }

            return builder.ToString();
        }
    }
}
