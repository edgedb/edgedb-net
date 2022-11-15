using EdgeDB.CIL.Interpreters;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EdgeDB.CIL
{
    internal abstract class BaseRefiner<TExpression> : IExpressionRefiner
        where TExpression : Expression
    {
        protected abstract Expression Refine(TExpression expression, RefiningContext context);

        protected Expression RefineOther(Expression expression, RefiningContext context)
            => ExpressionRefiner.RefineExpression(expression, context);

        Type IExpressionRefiner.ExpressionType => typeof(TExpression);

        Expression IExpressionRefiner.Refine(Expression expression, RefiningContext context)
            => Refine((TExpression)expression, context);
    }

    internal class ExpressionRefiner
    {
        private static readonly ConcurrentDictionary<Type, IExpressionRefiner> _refiners;

        static ExpressionRefiner()
        {
            _refiners = new(
                typeof(ExpressionRefiner).Assembly
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(IExpressionRefiner)) && !x.IsAbstract)
                .Select(x =>
                {
                    var inst = (IExpressionRefiner)Activator.CreateInstance(x)!;
                    return new KeyValuePair<Type, IExpressionRefiner>(inst.ExpressionType, inst);
                }));
        }

        public static Expression RefineExpression(Expression expression, RefiningContext context)
        {
            // if no refiners declared for this expression, pass thru
            var expType = expression.GetType();
            while (expType is not null && expType.IsNotPublic)
                expType = expType.BaseType;

            if (!_refiners.TryGetValue(expType ?? expression.GetType(), out var refiner))
                return expression;

            // refine the expression and return it
            return refiner.Refine(expression, context);
        }
    }

    internal interface IExpressionRefiner
    {
        Type ExpressionType { get; }

        Expression Refine(Expression expression, RefiningContext context);
    }
}

