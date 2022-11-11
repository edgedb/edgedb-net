using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class CopyObjInterpreter : BaseCILInterpreter
    {
        public CopyObjInterpreter()
            : base(OpCodeType.Cpobj)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: is this correct translation of cpobj?

            var source = context.Stack.PopExp();
            var dest = context.Stack.PopExp();

            return Expression.Assign(dest, source);
        }
    }
}

