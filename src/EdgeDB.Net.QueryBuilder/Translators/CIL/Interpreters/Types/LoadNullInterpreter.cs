using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadNullInterpreter : BaseCILInterpreter
    {
        public LoadNullInterpreter()
            : base(OpCodeType.Ldnull)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            return Expression.Constant(null);
        }
    }
}

