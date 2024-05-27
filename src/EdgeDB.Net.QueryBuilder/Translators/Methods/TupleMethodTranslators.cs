using System.Runtime.CompilerServices;

namespace EdgeDB.Translators.Methods;

internal sealed class TupleMethodTranslators : MethodTranslator
{
    public override Type TranslatorTargetType => typeof(Tuple);

    [MethodName(nameof(Tuple.Create))]
    public void Create(QueryWriter writer, params TranslatedParameter[] args)
    {
        writer.Wrapped(Token.Of(writer =>
        {
            for (int i = 0; i != args.Length - 1; i++)
            {
                writer.Append(args[i], ", ");
            }

            writer.Append(args[^1]);
        }));
    }
}
