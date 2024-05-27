using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.Builders;

internal delegate void SelectShapeExpressionTranslatorCallback(QueryWriter writer, ShapeElementExpression expression);

internal class SelectShape(IEnumerable<SelectedProperty> shape, Type type)
{
    private readonly SelectedProperty[] _shape = shape.ToArray();

    public void Compile(QueryWriter writer, SelectShapeExpressionTranslatorCallback translator) =>
        writer.Shape($"{type.GetEdgeDBTypeName()}_shape", _shape, (writer, x) =>
        {
            x.Compile(writer, translator);
        });
}

internal class SelectedProperty(MemberInfo member)
{
    public SelectedProperty(MemberInfo member, ShapeElementExpression value)
        : this(member)
    {
        ElementValue = value;
    }

    public SelectedProperty(MemberInfo member, SelectShape shape)
        : this(member)
    {
        ElementShape = shape;
    }

    public string Name { get; } = member.GetEdgeDBPropertyName();
    public ShapeElementExpression? ElementValue { get; }
    public SelectShape? ElementShape { get; }

    public void Compile(QueryWriter writer, SelectShapeExpressionTranslatorCallback translator)
    {
        if (ElementValue.HasValue)
        {
            writer.Append(Name, " := ");
            translator(writer, ElementValue.Value);
        }
        else if (ElementShape is not null)
        {
            writer.Append(Name, ": ");
            ElementShape.Compile(writer, translator);
        }
        else
        {
            writer.Append(Name);
        }
    }
}

internal readonly struct ShapeElementExpression
{
    //public readonly bool IsSelector;

    public readonly LambdaExpression Root;
    public readonly Expression Expression;

    public ShapeElementExpression(LambdaExpression root, Expression exp)
    {
        Root = root;
        Expression = exp;
    }
}

public abstract class BaseShapeBuilder : IShapeBuilder
{
    public BaseShapeBuilder(Type type)
    {
        SelectedType = type;

        LinkProperties = new Dictionary<string, (MemberInfo, bool)>();
        SelectedProperties = new Dictionary<string, SelectedProperty>();

        var allProps = type.GetEdgeDBTargetProperties();

        foreach (var prop in allProps)
            if (EdgeDBTypeUtils.IsLink(prop.PropertyType, out var isMulti, out _))
                LinkProperties.Add(prop.GetEdgeDBPropertyName(), (prop, isMulti));
            else
                SelectedProperties.Add(prop.GetEdgeDBPropertyName(), new SelectedProperty(prop));
    }

    internal Dictionary<string, SelectedProperty> SelectedProperties { get; set; }
    internal Dictionary<string, (MemberInfo, bool)> LinkProperties { get; set; }
    public Type SelectedType { get; }

    SelectShape IShapeBuilder.GetShape() => GetShape();

    public static BaseShapeBuilder CreateDefault(Type type)
        => new StaticShapeBuilder(type);

    internal static Dictionary<MemberInfo, ShapeElementExpression> FlattenAnonymousExpression(Type selectedType,
        LambdaExpression expression)
    {
        // new expression
        if (expression.Body is not NewExpression init || !init.Type.IsAnonymousType() || init.Members is null)
            throw new InvalidOperationException($"Expected anonymous object initializer, but got {expression.Body}");

        return FlattenNewExpression(selectedType, init, expression);
    }

    internal static Dictionary<MemberInfo, ShapeElementExpression> FlattenNewExpression(Type? selectedType,
        NewExpression expression, LambdaExpression root)
    {
        if (!expression.Type.IsAnonymousType() || expression.Members is null)
            throw new InvalidOperationException($"Expected anonymous object initializer, but got {expression}");

        var realProps =
            selectedType?.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? Array.Empty<PropertyInfo>();

        Dictionary<MemberInfo, ShapeElementExpression> dict = new();

        for (var i = 0; i != expression.Arguments.Count; i++)
        {
            var argExpression = expression.Arguments[i];
            var member = expression.Members[i];

            // cross reference the 'T' type and check for any explicit name or naming convention
            var realProp = realProps.FirstOrDefault(x => x.Name == member.Name);

            dict.Add(realProp ?? member, new ShapeElementExpression(root, argExpression));
        }

        return dict;
    }

    internal SelectShape GetShape() => new(SelectedProperties.Select(x => x.Value), SelectedType);

    private class StaticShapeBuilder : BaseShapeBuilder
    {
        public StaticShapeBuilder(Type type)
            : base(type)
        {
        }
    }
}

public sealed class ShapeBuilder<T> : BaseShapeBuilder
{
    public ShapeBuilder()
        : base(typeof(T))
    {
    }

    public ShapeBuilder<T> IncludeMultiLink<TIncluded>(Expression<Func<T, IEnumerable<TIncluded?>?>> selector)
        => IncludeInternal<TIncluded>(selector);

    public ShapeBuilder<T> IncludeMultiLink<TIncluded>(
        Expression<Func<T, IEnumerable<TIncluded?>?>> selector,
        Action<ShapeBuilder<TIncluded>> shape)
        => IncludeInternal(selector, shape);

    public ShapeBuilder<T> Include<TIncluded>(Expression<Func<T, TIncluded?>> selector)
        => IncludeInternal<TIncluded>(selector, errorOnMultiLink: true);

    public ShapeBuilder<T> Include<TIncluded>(Expression<Func<T, TIncluded?>> selector,
        Action<ShapeBuilder<TIncluded>> shape)
        => IncludeInternal(selector, shape, true);

    public ShapeBuilder<T> Exclude<TExcluded>(Expression<Func<T, TExcluded>> selector)
    {
        var member = ExpressionUtils.GetMemberSelection(selector);

        SelectedProperties.Remove(member.GetEdgeDBPropertyName());

        return this;
    }

    public ShapeBuilder<T> Computeds<TAnon>(Expression<Func<T, TAnon>> computedsSelector)
        => ComputedsInternal(computedsSelector);

    public ShapeBuilder<T> Computeds<TAnon>(Expression<Func<QueryContextSelf<T>, T, TAnon>> computedsSelector)
        => ComputedsInternal(computedsSelector);

    internal ShapeBuilder<T> ComputedsInternal(LambdaExpression expression)
    {
        var computeds = FlattenAnonymousExpression(SelectedType, expression);

        foreach (var computed in computeds)
            SelectedProperties[computed.Key.GetEdgeDBPropertyName()] =
                new SelectedProperty(computed.Key, computed.Value);

        return this;
    }

    public ShapeBuilder<T, TAnon> Explicitly<TAnon>(Expression<Func<T, TAnon>> explicitSelector)
        => ExplicitlyInternal<TAnon>(explicitSelector);

    public ShapeBuilder<T, TAnon> Explicitly<TAnon>(Expression<Func<QueryContextSelf<T>, T, TAnon>> explicitSelector)
        => ExplicitlyInternal<TAnon>(explicitSelector);


    internal ShapeBuilder<T, U> ExplicitlyInternal<U>(LambdaExpression expression)
    {
        var members = FlattenAnonymousExpression(SelectedType, expression);

        SelectedProperties.Clear();

        foreach (var member in members)
            SelectedProperties[member.Key.GetEdgeDBPropertyName()] = ParseShape(member.Key, member.Value);

        return new ShapeBuilder<T, U>(SelectedType, this);
    }

    private SelectedProperty ParseShape(MemberInfo info, ShapeElementExpression element)
    {
        // theres 3 types we could come across:
        // 1. Property: include the specified property in the shape; ex: 'Name = x.Name'
        // 2. Computed: include the compuded in the shape; ex: 'Name = {exp}'
        // 3. Subshape: include the sub shape; ex: 'Friend = new { Name = x.Friend.Name }

        if (element.Expression is MemberExpression)
        {
            var treeTail = ExpressionUtils.DisassembleExpression(element.Expression).Last();

            if (treeTail is ParameterExpression param && element.Root.Parameters.Contains(param))
            {
                return new SelectedProperty(info);
            }

            // treat as a computed
            return new SelectedProperty(info, element);
        }

        if (element.Expression is NewExpression newExpression)
        {
            // this is a subshape, try to get the type

            var flattened = FlattenNewExpression(info.GetMemberType(), newExpression, element.Root)
                .Select(x => ParseShape(x.Key, x.Value));

            return new SelectedProperty(info, new SelectShape(flattened, info.GetMemberType()));
        }

        // computed
        return new SelectedProperty(info, element);
    }

    private ShapeBuilder<T> IncludeInternal<TIncluded>(LambdaExpression selector,
        Action<ShapeBuilder<TIncluded>>? shape = null, bool errorOnMultiLink = false)
    {
        var member = ExpressionUtils.GetMemberSelection(selector);

        if (errorOnMultiLink && LinkProperties.TryGetValue(member.GetEdgeDBPropertyName(), out var info) && info.Item2)
            throw new InvalidOperationException("Use IncludeMultiLink for multi-link properties");

        if (LinkProperties.ContainsKey(member.GetEdgeDBPropertyName()))
        {
            var builder = new ShapeBuilder<TIncluded>();
            if (shape is not null)
                shape(builder);
            SelectedProperties.TryAdd(member.GetEdgeDBPropertyName(), new SelectedProperty(member, builder.GetShape()));
        }

        return this;
    }
}

public sealed class ShapeBuilder<T, TShape> : BaseShapeBuilder
{
    public ShapeBuilder(Type type, BaseShapeBuilder other) : base(type)
    {
        LinkProperties = other.LinkProperties;
        SelectedProperties = other.SelectedProperties;
    }
}

internal interface IShapeBuilder
{
    Type SelectedType { get; }
    internal SelectShape GetShape();
}
