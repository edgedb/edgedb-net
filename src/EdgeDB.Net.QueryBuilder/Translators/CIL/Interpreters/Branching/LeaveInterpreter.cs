using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LeaveInterpreter : BaseCILInterpreter
    {
        public LeaveInterpreter()
            : base(OpCodeType.Leave, OpCodeType.Leave_s)
        {

        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // do nothing
            return Expression.Empty();
        }
    }
}

