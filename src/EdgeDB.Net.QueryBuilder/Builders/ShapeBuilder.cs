
/* Unmerged change from project 'EdgeDB.Net.QueryBuilder (net6.0)'
Before:
using EdgeDB.Schema;
After:
using EdgeDB;
using EdgeDB.Builders;
using EdgeDB.Schema;
*/
using EdgeDB.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Builders
{
    internal delegate void SelectShapeExpressionTranslatorCallback(QueryWriter writer, ShapeElementExpression expression);

    internal class SelectShape
    {
        private readonly SelectedProperty[] _shape;

        private readonly Type _type;

        public SelectShape(IEnumerable<SelectedProperty> shape, Type type)
        {
            _shape = shape.ToArray();
            _type = type;
        }

        public void Compile(QueryWriter writer, SelectShapeExpressionTranslatorCallback translator)
        {
            writer.Shape($"{_type.GetEdgeDBTypeName()}_shape", _shape, (writer, x) =>
            {
                x.Compile(writer, translator);
            });
        }
    }

    internal class SelectedProperty
    {
        public string Name { get; }
        public ShapeElementExpression? ElementValue { get; }
        public SelectShape? ElementShape { get; }

        public SelectedProperty(MemberInfo member)
        {
            Name = member.GetEdgeDBPropertyName();
        }

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
        public static BaseShapeBuilder CreateDefault(Type type)
            => new StaticShapeBuilder(type);
        public Type SelectedType { get; private set; }

        internal readonly Dictionary<string, SelectedProperty> SelectedProperties;
        internal readonly Dictionary<string, (MemberInfo, bool)> LinkProperties;

        public BaseShapeBuilder(Type type)
        {
            SelectedType = type;

            LinkProperties = new();
            SelectedProperties = new();

            var allProps = type.GetEdgeDBTargetProperties();

            foreach (var prop in allProps)
                if (EdgeDBTypeUtils.IsLink(prop.PropertyType, out var isMulti, out _))
                    LinkProperties.Add(prop.GetEdgeDBPropertyName(), (prop, isMulti));
                else
                    SelectedProperties.Add(prop.GetEdgeDBPropertyName(), new(prop));
        }

        internal static Dictionary<MemberInfo, ShapeElementExpression> FlattenAnonymousExpression(Type selectedType, LambdaExpression expression)
        {
            // new expression
            if (expression.Body is not NewExpression init || !init.Type.IsAnonymousType() || init.Members is null)
                throw new InvalidOperationException($"Expected anonymous object initializer, but got {expression.Body}");

            return FlattenNewExpression(selectedType, init, expression);
        }

        internal static Dictionary<MemberInfo, ShapeElementExpression> FlattenNewExpression(Type? selectedType, NewExpression expression, LambdaExpression root)
        {
            if (!expression.Type.IsAnonymousType() || expression.Members is null)
                throw new InvalidOperationException($"Expected anonymous object initializer, but got {expression}");

            var realProps = selectedType?.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?? Array.Empty<PropertyInfo>();

            Dictionary<MemberInfo, ShapeElementExpression> dict = new();

            for (var i = 0; i != expression.Arguments.Count; i++)
            {
                var argExpression = expression.Arguments[i];
                var member = expression.Members[i];

                // cross reference the 'T' type and check for any explicit name or naming convention
                var realProp = realProps.FirstOrDefault(x => x.Name == member.Name);

                dict.Add(realProp ?? member, new(root, argExpression));
            }

            return dict;
        }

        internal static MemberInfo GetSelectedProperty(LambdaExpression expression)
        {
            if (expression.Body is not MemberExpression member)
                throw new InvalidOperationException("The body of the expression must be a member expression");

            return member.Member;
        }

        internal SelectShape GetShape()
        {
            return new(SelectedProperties.Select(x => x.Value), SelectedType);
        }

        SelectShape IShapeBuilder.GetShape() => GetShape();

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
        { }

        public ShapeBuilder<T> IncludeMultiLink<TIncluded>(Expression<Func<T, IEnumerable<TIncluded?>?>> selector)
            => IncludeInternal<TIncluded>(selector);

        public ShapeBuilder<T> IncludeMultiLink<TIncluded>(
            Expression<Func<T, IEnumerable<TIncluded?>?>> selector,
            Action<ShapeBuilder<TIncluded>> shape)
            => IncludeInternal(selector, shape);

        public ShapeBuilder<T> Include<TIncluded>(Expression<Func<T, TIncluded?>> selector)
            => IncludeInternal<TIncluded>(selector, errorOnMultiLink: true);

        public ShapeBuilder<T> Include<TIncluded>(Expression<Func<T, TIncluded?>> selector, Action<ShapeBuilder<TIncluded>> shape)
            => IncludeInternal(selector, shape, errorOnMultiLink: true);

        public ShapeBuilder<T> Exclude<TExcluded>(Expression<Func<T, TExcluded>> selector)
        {
            var member = GetSelectedProperty(selector);

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
                SelectedProperties[computed.Key.GetEdgeDBPropertyName()] = new(computed.Key, computed.Value);

            return this;
        }

        public ShapeBuilder<T> Explicitly<TAnon>(Expression<Func<T, TAnon>> explicitSelector)
            => ExplicitlyInternal(explicitSelector);

        public ShapeBuilder<T> Explicitly<TAnon>(Expression<Func<QueryContextSelf<T>, T, TAnon>> explicitSelector)
            => ExplicitlyInternal(explicitSelector);


        internal ShapeBuilder<T> ExplicitlyInternal(LambdaExpression expression)
        {
            var members = FlattenAnonymousExpression(SelectedType, expression);

            SelectedProperties.Clear();

            foreach (var member in members)
                SelectedProperties[member.Key.GetEdgeDBPropertyName()] = ParseShape(member.Key, member.Value);

            return this;
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

        private ShapeBuilder<T> IncludeInternal<TIncluded>(LambdaExpression selector, Action<ShapeBuilder<TIncluded>>? shape = null, bool errorOnMultiLink = false)
        {
            var member = GetSelectedProperty(selector);

            if (errorOnMultiLink && LinkProperties.TryGetValue(member.GetEdgeDBPropertyName(), out var info) && info.Item2)
                throw new InvalidOperationException("Use IncludeMultiLink for multi-link properties");

            if (LinkProperties.ContainsKey(member.GetEdgeDBPropertyName()))
            {
                var builder = new ShapeBuilder<TIncluded>();
                if (shape is not null)
                    shape(builder);
                SelectedProperties.TryAdd(member.GetEdgeDBPropertyName(), new(member, builder.GetShape()));
            }

            return this;
        }
    }

    internal interface IShapeBuilder
    {
        Type SelectedType { get; }
        internal SelectShape GetShape();
    }
}
