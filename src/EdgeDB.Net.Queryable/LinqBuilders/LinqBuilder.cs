using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.LinqBuilders
{
    internal static class LinqBuilder
    {
        private static ConcurrentDictionary<MethodInfo, Func<BaseLinqBuilder>> _builders;

        static LinqBuilder()
        {
            _builders = new(
                typeof(LinqBuilder).Assembly
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(BaseLinqBuilder)) && !x.IsAbstract)
                .Select(x => (BaseLinqBuilder)Activator.CreateInstance(x)!)
                .ToDictionary(
                    x => x.Method,
                    x => Expression.Lambda<Func<BaseLinqBuilder>>(
                        Expression.New(
                            x.GetType().GetConstructor(Type.EmptyTypes)!
                         )
                    ).Compile()
                )
            );
        }

        public static LinqBuilderHandle CreateBuilder()
        {
            return new();
        }

        public class LinqBuilderHandle : IDisposable
        {
            private List<BaseLinqBuilder> _nodes;

            private bool _disposed = false;

            public LinqBuilderHandle()
            {
                _nodes = new();
            }

            public void Process(EdgeDBQueryable queryable)
            {
                // get the linq method
                if (queryable.Expression is not MethodCallExpression mce)
                    throw new NotSupportedException("Needs MCE expression");

                var method = mce.Method.IsGenericMethod ? mce.Method.GetGenericMethodDefinition() : mce.Method;

                if (!_builders.TryGetValue(method, out var builderFactory))
                    throw new NotSupportedException($"No LINQ builder found for method {mce.Method.Name}");

                var node = builderFactory();

                node.Visit(new PartContext(queryable));

                _nodes.Add(node);
            }

            public GenericlessQueryBuilder Compile(GenericlessQueryBuilder builder)
            {
                for(int i = 0; i != _nodes.Count; i++)
                {
                    var node = _nodes[i];
                    var next = i == _nodes.Count - 1 ? null : _nodes[i+1];
                    node.Build(builder, next);
                }

                return builder;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                // clear all instance
                foreach(var instance in _nodes)
                {
                    if (instance is IDisposable ds)
                        ds.Dispose();
                }

                _nodes.Clear();
                _nodes = null!;

                _disposed = true;
            }
        }
    }
}
