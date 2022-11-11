using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class ArgListInterpreter : BaseCILInterpreter
    {
        public ArgListInterpreter()
            : base(OpCodeType.Arglist)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            throw new NotSupportedException("Cannot use 'arglist' (__arglist) directive in expressions");
        }
    }
}

