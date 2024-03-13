using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.Translators.Methods;

internal sealed class QueryContextTranslator : MethodTranslator<IQueryContext>
{
    internal override bool CanTranslate(Type type) => type.IsAssignableTo(typeof(IQueryContext));

    [MethodName(nameof(QueryContext.QueryArgument))]
    public void QueryArgument(QueryWriter writer, TranslatedParameter param)
    {
        writer.QueryArgument(
            EdgeDBTypeUtils.GetEdgeDBScalarOrTypeName(param.RawValue.Type),
            ExpressionTranslator.UnsafeExpressionAsString(param.RawValue)
        );
    }

    [MethodName(nameof(QueryContext.Global))]
    public void Global(QueryWriter writer, TranslatedParameter param, ExpressionContext context)
    {
        param.Context = context.Enter(x => x.StringWithoutQuotes = true);
        writer.Append(param);
    }

    [MethodName(nameof(QueryContext.Local))]
    public void Local(QueryWriter writer, TranslatedParameter param, ExpressionContext context)
    {
        var path = ExpressionTranslator.UnsafeExpressionAsString(param.RawValue)
            ?? throw new NullReferenceException("Expected parameter of 'local' to be notnull");

        var pathSegments = path.Split('.');

        writer.Append('.');

        for (var i = 0; i != pathSegments.Length; i++)
        {
            var prop = (MemberInfo?)context.LocalScope?.GetProperty(pathSegments[i]) ??
                       context.LocalScope?.GetField(pathSegments[i]) ??
                       (MemberInfo?)context.NodeContext.CurrentType.GetProperty(pathSegments[i]) ??
                       context.NodeContext.CurrentType.GetField(pathSegments[i]);

            if (prop is null)
                throw new InvalidOperationException(
                    $"The property \"{pathSegments[i]}\" within \"{path}\" is out of scope"
                );

            writer.Append(prop.GetEdgeDBPropertyName());

            if (i + 1 != pathSegments.Length)
                writer.Append('.');
        }
    }

    [MethodName(nameof(QueryContext.UnsafeLocal))]
    public void UnsafeLocal(QueryWriter writer, TranslatedParameter param)
    {
        writer.Append('.', ExpressionTranslator.UnsafeExpressionAsString(param.RawValue));
    }

    [MethodName(nameof(QueryContext.Raw))]
    public void Raw(QueryWriter writer, TranslatedParameter param)
    {
        writer.Append(ExpressionTranslator.UnsafeExpressionAsString(param.RawValue));
    }

    [MethodName(nameof(QueryContext.BackLink))]
    public void Backlink(QueryWriter writer, MethodCallExpression method, ExpressionContext context, params TranslatedParameter[] args)
    {
        var property = args[0];

        // depending on the backlink method called, we should set some flags:
        // whether or not the called function is using the string form or the lambda form
        var isRawPropertyName = property.ParameterType == typeof(string);

        // whether or not a shape argument was supplied
        var hasShape = !isRawPropertyName && args.Length > 1;

        writer.Append(".<");

        // translate the backlink property accessor
        property.Context = context.Enter(x =>
        {
            x.StringWithoutQuotes = isRawPropertyName;
            x.IncludeSelfReference = false;
        });

        writer.Append(property);

        // if its a lambda, add the corresponding generic type as a [is x] statement
        if (!isRawPropertyName)
        {
            writer
                .Append("[is ")
                .Append(method.Method.GetGenericArguments()[0].GetEdgeDBTypeName())
                .Append(']');
        }

        // if it has a shape, translate the shape and add it to the backlink
        if (hasShape)
        {
            writer.Wrapped(args[1], "{}", true);
        }
    }

    [MethodName(nameof(QueryContext.SubQuery))]
    [MethodName(nameof(QueryContext.SubQuerySingle))]
    public void SubQuery(QueryWriter writer, TranslatedParameter param, ExpressionContext context)
    {
        var builder = (IQueryBuilder)Expression.Lambda(param.RawValue).Compile().DynamicInvoke()!;
        writer.Wrapped(writer => builder.WriteTo(writer, context));
    }

    [MethodName(nameof(QueryContext.Aggregate))]
    public void Aggregate(QueryWriter writer, TranslatedParameter source, TranslatedParameter expressive)
    {
        if (expressive.RawValue is not LambdaExpression lambda)
            throw new NotSupportedException("The expressive operand of 'Ref' must be a lambda function");

        // we just prefix all references of the parameter to the func with the source, example:
        // Aggregate(People, (Person x) => EdgeQL.Count(x.Name))
        // will become
        // std::count(.people.name)
        // since .people is a set, .people.name returns a set
        expressive.Context = expressive.Context.Enter(x =>
        {
            x.ParameterPrefixes.Add(lambda.Parameters[0], writer => writer.Append(source, '.'));
            x.IncludeSelfReference = true;
        });

        writer.Append(expressive);
    }
}
