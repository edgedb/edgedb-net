using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryContext
    {
        public Type? ParameterType { get; set; }
        public string? ParameterName { get; set; }
        public Expression? Body { get; set; }
        public QueryContext? Parent { get; set; }
        public int? ParameterIndex { get; set; }
        public bool IsCharContext { get; set; } = false;
        public bool AllowStaticOperators { get; set; } = false;
        public bool IncludeSetOperand { get; set; } = true;
        public QueryBuilderContext? BuilderContext { get; set; }
        public Type? BindingType { get; set; }
        public bool AllowSubQueryGeneration { get; set; } = false;

        public bool IsVariableReference
            => Parent?.Body is MethodCallExpression mc && mc.Method.GetCustomAttribute<Operators.EquivalentOperator>()?.Operator?.GetType() == typeof(Operators.VariablesReference);
        
        
        public QueryContext() { }

        public virtual QueryContext Enter(Expression x, int? paramIndex = null, Action<QueryContext>? modifier = null)
        {
            var context = new QueryContext()
            {
                Body = x,
                ParameterName = ParameterName,
                ParameterType = ParameterType,
                IsCharContext = IsCharContext,
                ParameterIndex = paramIndex,
                Parent = this,
                BuilderContext = BuilderContext,
            };

            if (modifier != null)
                modifier(context);

            return context;
        }
    }

    internal class QueryContext<TInner, TReturn> : QueryContext
    {
        public QueryContext() { }
        public QueryContext(Expression<Func<TInner, TReturn>> func) : base()
        {
            Body = func.Body;
            ParameterType = func.Parameters[0].Type;
            ParameterName = func.Parameters[0].Name;
        }

        public QueryContext<TInner, TReturn> Enter(Expression x, int? paramIndex = null, Action<QueryContext<TInner, TReturn>>? modifier = null)
        {
            var context = new QueryContext<TInner, TReturn>()
            {
                Body = x,
                ParameterName = ParameterName,
                ParameterType = ParameterType,
                IsCharContext = IsCharContext,
                ParameterIndex = paramIndex,
                Parent = this
            };

            if (modifier != null)
                modifier(context);

            return context;
        }
    }

    internal class QueryContext<TReturn> : QueryContext
    {
        public QueryContext() { }
        public QueryContext(Expression<Func<TReturn>> func) : base()
        {
            Body = func.Body;
        }

        public QueryContext<TReturn> Enter(Expression x, int? paramIndex = null)
        {
            return new QueryContext<TReturn>()
            {
                Body = x,
                ParameterName = ParameterName,
                ParameterType = ParameterType,
                IsCharContext = IsCharContext,
                ParameterIndex = paramIndex,
                Parent = this
            };
        }
    }
}
