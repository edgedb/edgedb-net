using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class SwitchInterpreter : BaseCILInterpreter
    {
        public SwitchInterpreter()
            : base(OpCodeType.Switch)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            throw new NotSupportedException("Cannot use switch for methods translating to edgeql");
        }
    }
}

