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
        private static readonly Dictionary<ExpressionType, Operator> _expOperators;
        private static readonly Dictionary<string, Operator> _operators;

        private class Operator
        {
            public string Name { get; }
            public int ParameterCount { get; }

            private readonly MethodInfo _method;

            public Operator(string name, int paramCount, MethodInfo method)
            {
                Name = name;
                ParameterCount = paramCount;
                _method = method;
            }

            public string Build(params object?[] args)
            {
                return (string)_method.Invoke(null, args)!;
            }
        }

        static Grammar()
        {
            var operators = typeof(Grammar).GetMethods()
                .Where(x => x.GetCustomAttribute<EdgeQLOpAttribute>() is not null);

            _operators = new();
            _expOperators = new();

            foreach (var op in operators)
            {
                var opAttr = op.GetCustomAttribute<EdgeQLOpAttribute>()!;

                if (_operators.ContainsKey(opAttr.Operator))
                    continue;

                var expAttr = op.GetCustomAttribute<EquivalentExpressionAttribute>();

                var opInfo = new Operator(opAttr.Operator, op.GetParameters().Length, op);

                _operators.Add(opAttr.Operator, opInfo);

                if (expAttr is not null)
                {
                    foreach (var exp in expAttr.Expressions)
                    {
                        if (!_expOperators.ContainsKey(exp))
                        {
                            _expOperators.Add(exp, opInfo);
                        }
                    }
                }
            }
        }

        public static bool TryBuildOperator(ExpressionType type, StringBuilder result, params TranslatorProxy[] args)
        {
            if(_expOperators.TryGetValue(type, out var op))
            {
                result = op.Build(args);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryBuildOperator(string opName, StringBuilder result, params object?[] args)
        {
            if (_operators.TryGetValue(opName, out var op))
            {
                result = op.Build(args);
                return true;
            }

            result = null;
            return false;
        }
    }
}
