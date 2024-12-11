using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.LinqBuilders
{
    internal abstract class BaseLinqBuilder
    {
        internal readonly MethodInfo Method;

        public BaseLinqBuilder(MethodInfo method)
        {
            Method = method;
        }

        public abstract void Visit(PartContext queryable);
        public abstract void Build(GenericlessQueryBuilder builder, BaseLinqBuilder? nextNode);

        protected MethodCallExpression AsMethodCall(Expression expression)
        {
            if (expression is not MethodCallExpression mce)
                throw new NotSupportedException($"Expected MethodCallExpression but got {expression}");

            return mce;
        }

        protected Expression Unquote(Expression expression)
        {
            var exp = expression;

            while(exp.NodeType is ExpressionType.Quote && exp is UnaryExpression unary)
            {
                exp = unary.Operand;
            }

            return exp;
        }
    }
}
