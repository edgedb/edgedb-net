using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class EndFinally : BaseCILInterpreter
    {
        public EndFinally()
            : base(OpCodeType.Endfinally)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            throw new NotSupportedException("Cannot use finally block in functions translated to edgeql");
        }
    }
}

