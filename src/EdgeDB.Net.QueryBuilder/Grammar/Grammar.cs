using EdgeDB.Translators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal partial class Grammar
    {
        private static readonly List<Operator> _operators;

        private class Operator
        {
            public ExpressionType[] ExpressionTypes { get; }
            public string Name { get; }
            public int ParameterCount { get; }

            private readonly MethodInfo _method;

            public Operator(ExpressionType[] expressionTypes, string name, int paramCount, MethodInfo method)
            {
                ExpressionTypes = expressionTypes;
                Name = name;
                ParameterCount = paramCount;
                _method = method;
            }

            public void Build(QueryStringWriter writer, params QueryStringWriter.Proxy[] args)
            {
                var arr = new object?[args.Length + 1];
                arr[0] = writer;
                args.CopyTo(arr, 1);
                _method.Invoke(null, arr);
            }
        }

        static Grammar()
        {
            var operators = typeof(Grammar).GetMethods()
                .Where(x => x.GetCustomAttribute<EdgeQLOpAttribute>() is not null);

            _operators = new();

            foreach (var op in operators)
            {
                var opAttr = op.GetCustomAttribute<EdgeQLOpAttribute>()!;

                var expAttr = op.GetCustomAttribute<EquivalentExpressionAttribute>();

                var expressions = expAttr is null
                    ? Array.Empty<ExpressionType>()
                    : expAttr.Expressions;

                var opInfo = new Operator(expressions, opAttr.Operator, op.GetParameters().Length - 1, op);

                _operators.Add(opInfo);
            }
        }

        private static Operator? SearchForBestMatch(ExpressionType type, QueryStringWriter.Proxy[] args)
            => _operators.FirstOrDefault(x => x.ExpressionTypes.Contains(type) && x.ParameterCount == args.Length);

        public static bool TryBuildOperator(ExpressionType type, QueryStringWriter writer,
            params QueryStringWriter.Proxy[] args)
        {
            var op = SearchForBestMatch(type, args);

            if (op is null)
                return false;

            op.Build(writer, args);
            return true;
        }

        public static bool TryBuildOperator(ExpressionType type, QueryStringWriter writer, params object?[] args)
            => TryBuildOperator(
                type,
                writer,
                args.Select(x => new QueryStringWriter.Proxy(writer => writer
                    .Append(x)
                )).ToArray()
            );
    }
}
