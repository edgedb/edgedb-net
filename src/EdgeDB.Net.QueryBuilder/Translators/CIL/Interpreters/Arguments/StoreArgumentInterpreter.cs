using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreArgumentInterpreter : BaseCILInterpreter
    {
        public StoreArgumentInterpreter()
            : base(OpCodeType.Starg, OpCodeType.Starg_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var index = (short)instruction.Oprand!;
            var value = context.Stack.PopExp();
            return Expression.Assign(context.Parameters[index], value);
        }
    }
}

