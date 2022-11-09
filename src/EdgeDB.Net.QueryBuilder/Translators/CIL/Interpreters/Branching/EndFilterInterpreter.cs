using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class EndFilterInterpreter : BaseCILInterpreter
    {
        public EndFilterInterpreter()
            : base(OpCodeType.Endfilter)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            throw new NotSupportedException("Cannot use try-catch features in functions that are translated to edgeql");
        }
    }
}

