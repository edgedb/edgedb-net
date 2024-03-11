namespace EdgeDB.Translators.Methods;

internal sealed class GroupMethodTranslator : MethodTranslator<EdgeQL>
{
    [MethodName(nameof(EdgeQL.Cube))]
    public void Cube(QueryWriter writer, TranslatedParameter newExp)
    {
        newExp.Context = newExp.Context.Enter(x => x.WrapNewExpressionInBrackets = false);
        writer.Function("cube", newExp);
    }

    [MethodName(nameof(EdgeQL.Rollup))]
    public void Rollup(QueryWriter writer, TranslatedParameter newExp)
    {
        newExp.Context = newExp.Context.Enter(x => x.WrapNewExpressionInBrackets = false);
        writer.Function("rollup", newExp);
    }
}
