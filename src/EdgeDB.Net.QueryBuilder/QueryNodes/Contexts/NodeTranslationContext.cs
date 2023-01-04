using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes.Contexts
{
    internal sealed class NodeTranslationContext
    {
        private readonly List<QueryGlobal> _globals;
        private readonly List<QueryGlobal> _prevGlobals;
        private readonly QueryNode _node;

        public NodeTranslationContext(QueryNode node)
        {
            _globals = new(node.Builder.QueryGlobals);
            _prevGlobals = new(node.Builder.QueryGlobals);
            _node = node;
        }

        public ContextConsumer CreateContextConsumer(LambdaExpression expression)
        {
            return new(SetTrackedReferences, _node, expression, _node.Builder.QueryVariables, _globals);
        }

        private void SetTrackedReferences()
        {
            foreach (var addedGlobal in _globals.Except(_prevGlobals))
            {
                _node.ReferencedGlobals.AddRange(_globals);
                _node.Builder.QueryGlobals.Add(addedGlobal);
            }
        }
    }

    internal sealed class ContextConsumer : ExpressionContext, IDisposable
    {
        private readonly Action _callback;

        public ContextConsumer(
            Action callback,
            QueryNode node,
            LambdaExpression rootExpression,
            IDictionary<string, object?> queryArguments,
            List<QueryGlobal> globals)
            : base(node.Context, rootExpression, queryArguments, globals)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            _callback();
        }
    }
}
