using EdgeDB.Binary.Protocol.Common.Descriptors;
using EdgeDB.Binary.Protocol.V2._0.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Codecs
{
    internal static class CodecFormatter
    {
        public static StringBuilder Format(ICodec codec, int spacing = 2)
        {
            StringBuilder sb = new();

            var depth = 0;

            Process(codec, ref sb, in spacing, ref depth);

            return sb;
        }

        private static string Padding(in int depth, string s)
            => "".PadLeft(depth) + s;

        private static void Process(
            ICodec codec,
            scoped ref StringBuilder builder,
            scoped in int spacing,
            scoped ref int depth)
        {
            switch (codec)
            {
                case IWrappingCodec wrapping:
                    builder.AppendLine(Padding(in depth, $"<{codec.Id}> {codec} {{"));
                    depth += spacing;
                    builder.Append(FormatMetadata(codec.Metadata, ref depth, in spacing));
                    builder.AppendLine(Padding(in depth, "child: {"));
                    depth += spacing;
                    Process(wrapping.InnerCodec, ref builder, in spacing, ref depth);
                    depth -= spacing;
                    builder.AppendLine(Padding(in depth, "}"));
                    depth -= spacing;
                    builder.AppendLine(Padding(in depth, "}"));
                    break;
                case IMultiWrappingCodec multiWrapping:
                    var names = multiWrapping switch
                    {
                        ObjectCodec obj => obj.PropertyNames,
                        SparceObjectCodec sparse => sparse.FieldNames,
                        _ => null
                    };

                    builder.AppendLine(Padding(in depth, $"<{codec.Id}> {codec} {{"));

                    depth += spacing;
                    builder.Append(FormatMetadata(codec.Metadata, ref depth, in spacing));
                    
                    if(multiWrapping.InnerCodecs.Length == 0)
                    {
                        builder.AppendLine(Padding(in depth, "children: []"));

                        if(codec.Metadata is not null)
                        {
                            depth -= spacing;

                            builder.AppendLine(Padding(in depth, "}"));
                        }
                    }
                    else
                    {
                        builder.AppendLine(Padding(in depth, "children: ["));
                        depth += spacing;
                        for (int i = 0; i != multiWrapping.InnerCodecs.Length; i++)
                        {
                            var pos = builder.Length;
                            Process(multiWrapping.InnerCodecs[i], ref builder, in spacing, ref depth);

                            if (names is not null)
                                builder.Insert(pos + depth, $"{names[i]}: ");
                        }
                        depth -= codec.Metadata is not null ? spacing * 2 : spacing;
                        builder.AppendLine(Padding(in depth, "]"));
                    }
                    
                    depth -= spacing;
                    builder.AppendLine(Padding(in depth, "}"));
                    break;
                case CompilableWrappingCodec compilable:
                    builder.AppendLine(Padding(in depth, $"<{codec.Id}> {codec} {{"));
                    depth += spacing;
                    builder.Append(FormatMetadata(codec.Metadata, ref depth, in spacing));
                    builder.AppendLine(Padding(in depth, "child: {"));
                    depth += spacing;
                    Process(compilable.InnerCodec, ref builder, in spacing, ref depth);
                    depth -= spacing;
                    builder.AppendLine(Padding(in depth, "}"));
                    depth -= spacing;
                    builder.AppendLine(Padding(in depth, "}"));
                    break;
                default:
                    var metadata = FormatMetadata(codec.Metadata, ref depth, in spacing);

                    if(metadata is null)
                        builder.AppendLine(Padding(in depth, $"<{codec.Id}> {codec}"));
                    else
                    {
                        builder.AppendLine(Padding(in depth, $"<{codec.Id}> {codec} {{"));
                        depth += spacing;
                        builder.Append(metadata);
                        depth -= spacing;
                    }
                    break;
            }
        }

        private static StringBuilder? FormatMetadata(CodecMetadata? metadata, scoped ref int depth, scoped in int spacing)
        {
            if (metadata is null)
                return null;

            var sb = new StringBuilder();

            sb.AppendLine(Padding(in depth, "metadata: {"));

            depth += spacing;

            sb.Append(Padding(in depth, "schema_name: "));
            sb.Append(metadata.SchemaName ?? "NULL");
            sb.AppendLine(",");

            sb.Append(Padding(in depth, "is_schema_defined: "));
            sb.Append(metadata.IsSchemaDefined);
            sb.AppendLine(",");
            sb.Append(Padding(in depth, "ancestors: ["));

            if (metadata.Ancestors is null || metadata.Ancestors.Length == 0)
                sb.AppendLine("],");
            else
            {
                depth += spacing;
                sb.AppendLine();

                for(var i = 0; i != metadata.Ancestors.Length; i++)
                {
                    var ancestor = metadata.Ancestors[i];

                    var name = ancestor.Codec is not null
                        ? ancestor.Codec.GetType().Name
                        : ancestor.Descriptor.GetType().Name;

                    sb.AppendLine(Padding(in depth, $"[{(ancestor.Codec?.Id ?? ancestor.Descriptor.Id)}]: {name}"));
                }

                depth -= spacing;
                sb.AppendLine(Padding(in depth, "],"));
            }

            return sb;
        }
    }
}
