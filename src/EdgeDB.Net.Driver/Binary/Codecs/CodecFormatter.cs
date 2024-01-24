using EdgeDB.Binary.Protocol.Common.Descriptors;
using System.Text;

namespace EdgeDB.Binary.Codecs;

internal static class CodecFormatter
{
    public static StringBuilder FormatCodecAsTree(ICodec codec)
    {
        var sb = new StringBuilder();

        AppendCodecToTree(sb, codec, 0);

        return sb;
    }


    private const int CODEC_TREE_SPACING = 2;
    private static void AppendCodecToTree(StringBuilder tree, ICodec codec, int depth, string? prefix = null)
    {
        tree.AppendLine("".PadLeft(depth) + $"{prefix} {codec}");

        if (codec is IMultiWrappingCodec multiwrap)
        {
            for (int i = 0; i != multiwrap.InnerCodecs.Length; i++)
            {
                var innerCodec = multiwrap.InnerCodecs[i];
                AppendCodecToTree(
                    tree,
                    innerCodec,
                    depth + CODEC_TREE_SPACING,
                    i == multiwrap.InnerCodecs.Length - 1 ? "\u2514" : "\u251c"
                );
            }
        }
        else if (codec is IWrappingCodec wrapping)
        {
            AppendCodecToTree(
                tree,
                wrapping.InnerCodec,
                depth + CODEC_TREE_SPACING,
                "\u2514"
            );
        }
    }

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

                if (multiWrapping.InnerCodecs.Length == 0)
                {
                    builder.AppendLine(Padding(in depth, "children: []"));

                    if (codec.Metadata is not null)
                    {
                        depth -= spacing;

                        builder.AppendLine(Padding(in depth, "}"));
                    }
                }
                else
                {
                    builder.AppendLine(Padding(in depth, "children: ["));
                    depth += spacing;
                    for (var i = 0; i != multiWrapping.InnerCodecs.Length; i++)
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

                if (metadata is null)
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

            for (var i = 0; i != metadata.Ancestors.Length; i++)
            {
                var ancestor = metadata.Ancestors[i];

                var name = ancestor.Codec is not null
                    ? ancestor.Codec.GetType().Name
                    : ancestor.Descriptor.GetType().Name;

                sb.AppendLine(Padding(in depth, $"[{ancestor.Codec?.Id ?? ancestor.Descriptor.Id}]: {name}"));
            }

            depth -= spacing;
            sb.AppendLine(Padding(in depth, "],"));
        }

        return sb;
    }
}
