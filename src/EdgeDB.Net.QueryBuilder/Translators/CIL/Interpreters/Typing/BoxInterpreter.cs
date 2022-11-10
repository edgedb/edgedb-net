using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BoxInterpreter : BaseCILInterpreter
    {
        public BoxInterpreter()
            : base(OpCodeType.Box)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // do nothing as theres no need to box value types in expressions
            return context.ExpressionStack.Pop();
        }
    }
}

