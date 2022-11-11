using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadFieldInterpreter : BaseCILInterpreter
    {
        public LoadFieldInterpreter()
            : base(OpCodeType.Ldsflda, OpCodeType.Ldflda)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // pop the instance off the stack if its non-static
            if (instruction.OpCodeType is OpCodeType.Ldflda)
                context.Stack.PopExp();

            // resolve the field
            var field = instruction.OprandAsField();

            // push the field onto the member stack for
            // later use.
            context.Stack.Push(field);

            return Expression.Empty();
        }
    }
}

