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

        public Expression RootDelegateTarget { get; }

        public ParameterExpression[] Locals { get; }
        public ParameterExpression[] Parameters { get; }
        public InterpreterStack Stack { get; set; }
        public ILReader Reader { get; }

        public CILInterpreterContext(
            object? rootDelegateTarget,
            ILReader reader,
            InterpreterStack stack,
            ParameterExpression[] locals,
            ParameterExpression[] parameters)
        {
            RootDelegateTarget = Expression.Constant(rootDelegateTarget);
            Reader = reader;
            Stack = stack;
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

