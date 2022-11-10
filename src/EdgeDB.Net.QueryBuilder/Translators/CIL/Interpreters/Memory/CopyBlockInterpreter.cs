using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class CopyBlockInterpreter : BaseCILInterpreter
    {
        public CopyBlockInterpreter()
            : base(OpCodeType.Cpblk)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // Could be interpreted as Unsafe.CopyBlock()
            throw new NotSupportedException("Cannot use cpblk in edgeql");
        }
    }
}

