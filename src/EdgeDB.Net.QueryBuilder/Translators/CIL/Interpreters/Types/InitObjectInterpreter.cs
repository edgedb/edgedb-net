using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class InitObjectInterpreter : BaseCILInterpreter
    {
        public InitObjectInterpreter()
            : base(OpCodeType.Initblk)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // new expression for value type, no constructor
            var type = instruction.OprandAsType();

            return Expression.New(type);
        }
    }
}

