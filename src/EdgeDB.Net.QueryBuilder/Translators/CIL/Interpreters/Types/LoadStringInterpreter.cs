using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadStringInterpreter : BaseCILInterpreter
    {
        public LoadStringInterpreter()
            : base(OpCodeType.Ldstr)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var str = instruction.OprandAsString();
            return Expression.Constant(str);
        }
    }
}

