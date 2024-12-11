namespace EdgeDB.Translators.Methods;

internal sealed class EdgeQLMethodTranslator : MethodTranslator<EdgeQL>
{
    [MethodName(nameof(EdgeQL.Global))]
    public void Global(QueryWriter writer, TranslatedParameter name)
    {
        writer.Append("global ", name);
    }
}
