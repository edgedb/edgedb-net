using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class NotInterpreter : BaseCILInterpreter
    {
        public NotInterpreter()
            : base(OpCodeType.Not)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.Pop();
            return Expression.Not(value);
        }
    }
}

