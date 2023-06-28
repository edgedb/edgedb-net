using EdgeDB.QueryNodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.QueryBuilder.Expressions
{
    internal static class ExpressionTester
    {
        private static readonly LambdaExpression _emptyRoot = Expression.Lambda(Expression.Empty());

        private class MockNodeContext : NodeContext
        {
            public MockNodeContext(Type currentType) : base(currentType)
            {
            }
        }


        public static void AssertExpression<T>(
            Expression expression, string edgeql,
            LambdaExpression? root = null,
            NodeContext? nodeContext = null,
            Dictionary<string, object?>? args = null,
            List<QueryGlobal>? globals = null,
            QueryNode? node = null,
            Action<ExpressionContext>? contextDelegate = null)
        {
            root ??= expression is LambdaExpression lambda ? lambda : _emptyRoot;

            var context = new ExpressionContext(
                nodeContext ?? new MockNodeContext(typeof(T)),
                root,
                args ?? new Dictionary<string, object?>(),
                globals ?? new List<QueryGlobal>(),
                node
            );

            if(contextDelegate != null)
            {
                contextDelegate(context);
            }

            AssertExpression(() => context, expression, edgeql);
        }

        public static void AssertExpression(Func<ExpressionContext> contextFactory, Expression expression, string expected)
        {
            var result = ExpressionTranslator.ContextualTranslate(expression, contextFactory());

            Assert.AreEqual(expected, result);
        }

        public static void AssertException<T, TException>(
            LambdaExpression root, Expression expression, string edgeql,
            NodeContext? nodeContext = null,
            Dictionary<string, object?>? args = null,
            List<QueryGlobal>? globals = null,
            QueryNode? node = null)
            where TException : Exception
        {
            var context = new ExpressionContext(
                nodeContext ?? new MockNodeContext(typeof(T)),
                root,
                args ?? new Dictionary<string, object?>(),
                globals ?? new List<QueryGlobal>(),
                node
            );

            AssertException<TException>(() => context, expression);
        }

        public static void AssertException<TException>(Func<ExpressionContext> contextFactory, Expression expression)
            where TException : Exception
        {
            Assert.ThrowsException<TException>(() => ExpressionTranslator.ContextualTranslate(expression, contextFactory()));
        }
    }
}
