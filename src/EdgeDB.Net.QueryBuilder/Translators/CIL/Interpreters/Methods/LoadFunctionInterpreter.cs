using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadFunctionInterpreter : BaseCILInterpreter
    {
        public LoadFunctionInterpreter()
            : base(OpCodeType.Ldftn)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: figure out how to get MethodInfo from native pointer
            throw new NotImplementedException();
        }
    }
}

