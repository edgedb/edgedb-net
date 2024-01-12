
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
    internal delegate void SelectShapeExpressionTranslatorCallback(QueryStringWriter writer, ShapeElementExpression expression);

    internal class SelectShape
    {
        private readonly SelectedProperty[] _shape;

        public SelectShape(IEnumerable<SelectedProperty> shape)
        {
            _shape = shape.ToArray();
        }

        public void Compile(QueryStringWriter writer, SelectShapeExpressionTranslatorCallback translator)
        {
            writer.Append("{ ");

            for (int i = 0; i != _shape.Length;)
            {
                _shape[i].Compile(writer, translator);

                if (++i != _shape.Length)
                    writer.Append(", ");
            }

            writer.Append(" }");
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

        public void Compile(QueryStringWriter writer, SelectShapeExpressionTranslatorCallback translator)
        {
            if (ElementValue.HasValue)
            {
                writer
                    .Append(Name)
                    .Append(" := ");
                translator(writer, ElementValue.Value);
            }
            else if (ElementShape is not null)
            {
                writer
                    .Append(Name)
                    .Append(": ");
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

            var realProps = selectedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Dictionary<MemberInfo, ShapeElementExpression> dict = new();

            for (var i = 0; i != init.Arguments.Count; i++)
            {
                var argExpression = init.Arguments[i];
                var member = init.Members[i];

                // cross reference the 'T' type and check for any explicit name or naming convention
                var realProp = realProps.FirstOrDefault(x => x.Name == member.Name);

                dict.Add(realProp ?? member, new(expression, argExpression));
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
            return new(SelectedProperties.Select(x => x.Value));
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
        {
            var computeds = FlattenAnonymousExpression(SelectedType, computedsSelector);

            foreach (var computed in computeds)
                SelectedProperties[computed.Key.GetEdgeDBPropertyName()] = new(computed.Key, computed.Value);

            return this;
        }

        public ShapeBuilder<T> Computeds<TAnon>(Expression<Func<QueryContext<T>, T, TAnon>> computedsSelector)
        {
            var computeds = FlattenAnonymousExpression(SelectedType, computedsSelector);

            foreach (var computed in computeds)
                SelectedProperties[computed.Key.GetEdgeDBPropertyName()] = new(computed.Key, computed.Value);

            return this;
        }

        public ShapeBuilder<T> Explicitly<TAnon>(Expression<Func<T, TAnon>> explicitSelector)
        {
            var members = FlattenAnonymousExpression(SelectedType, explicitSelector);

            SelectedProperties.Clear();

            foreach (var member in members)
                SelectedProperties[member.Key.GetEdgeDBPropertyName()] = new(member.Key, member.Value);

            return this;
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
