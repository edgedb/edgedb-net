using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadFieldValueInterpreter : BaseCILInterpreter
    {
        public LoadFieldValueInterpreter()
            : base(OpCodeType.Ldfld, OpCodeType.Ldsfld)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var instance = instruction.OpCodeType is OpCodeType.Ldfld
                ? context.Stack.PopExp()
                : null;

            var field = instruction.OprandAsField();

            return Expression.Field(instance, field);
        }
    }
}

