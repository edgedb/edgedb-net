using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadLengthInterpreter : BaseCILInterpreter
    {
        public LoadLengthInterpreter()
            : base(OpCodeType.Ldlen)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var array = context.Stack.PopExp();

            return Expression.ArrayLength(array);
        }
    }
}

