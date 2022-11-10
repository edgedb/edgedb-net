using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace EdgeDB.CIL.Interpreters
{
    internal class CILInterpreterContext
    {
        public MethodBase RootMethod
            => Reader.MethodBase;

        public ParameterExpression[] Locals { get; }
        public ParameterExpression[] Parameters { get; }
        public Stack<Expression> ExpressionStack { get; set; }
        public Stack<MemberInfo> MemberStack { get; set; }
        public ILReader Reader { get; }

        public bool IsTailCall { get; set; }

        public CILInterpreterContext(
            ILReader reader,
            Stack<Expression> expressionStack,
            Stack<MemberInfo> memberStack,
            ParameterExpression[] locals,
            ParameterExpression[] parameters)
        {
            Reader = reader;
            ExpressionStack = expressionStack;
            MemberStack = memberStack;
            Locals = locals;
            Parameters = parameters;
        }

        public CILInterpreterContext Enter(Action<CILInterpreterContext> func)
        {
            var clone = (CILInterpreterContext)this.MemberwiseClone();
            func(clone);
            return clone;
        }
    }
}

