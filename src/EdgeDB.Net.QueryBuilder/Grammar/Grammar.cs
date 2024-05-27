using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB;

internal partial class Grammar
{
    private static readonly List<Operator> _operators;

    static Grammar()
    {
        var operators = typeof(Grammar).GetMethods()
            .Where(x => x.GetCustomAttribute<EdgeQLOpAttribute>() is not null);

        _operators = new List<Operator>();

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

    private static Operator? SearchForBestMatch(ExpressionType type, WriterProxy[] args)
        => _operators.FirstOrDefault(x => x.ExpressionTypes.Contains(type) && x.ParameterCount == args.Length);

    public static bool TryBuildOperator(ExpressionType type, QueryWriter writer,
        params WriterProxy[] args)
    {
        var op = SearchForBestMatch(type, args);

        if (op is null)
            return false;

        op.Build(writer, args);
        return true;
    }

    private class Operator
    {
        private readonly MethodInfo _method;

        public Operator(ExpressionType[] expressionTypes, string name, int paramCount, MethodInfo method)
        {
            ExpressionTypes = expressionTypes;
            Name = name;
            ParameterCount = paramCount;
            _method = method;
        }

        public ExpressionType[] ExpressionTypes { get; }
        public string Name { get; }
        public int ParameterCount { get; }

        public void Build(QueryWriter writer, params WriterProxy[] args) =>
            writer.Term(
                TermType.BinaryOp,
                "binary_op_translation",
                Defer.This(() => $"Called from {Name}({ParameterCount}) using method {_method.Name}"),
                new BinaryOpMetadata(ExpressionTypes),
                Token.Of(writer =>
                {
                    var arr = new object?[args.Length + 1];
                    arr[0] = writer;
                    args.CopyTo(arr, 1);
                    _method.Invoke(null, arr);
                })
            );
    }

    // public static bool TryBuildOperator(ExpressionType type, QueryWriter writer, params object?[] args)
    //     => TryBuildOperator(
    //         type,
    //         writer,
    //         args.Select(x => new WriterProxy(writer => writer
    //             .Append(x)
    //         )).ToArray()
    //     );
}
