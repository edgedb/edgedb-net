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
            var array = context.ExpressionStack.Pop();

            return Expression.ArrayLength(array);
        }
    }
}

