using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LocallocInterpreter : BaseCILInterpreter
    {
        public LocallocInterpreter()
            : base(OpCodeType.Localloc)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            throw new NotSupportedException("Cannot build expression from localloc instruction");
        }
    }
}

