using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadElementInterpreter : BaseCILInterpreter
    {
        public LoadElementInterpreter()
            : base(
                  OpCodeType.Ldelem,
                  OpCodeType.Ldelem_ref,
                  OpCodeType.Ldelem_i,
                  OpCodeType.Ldelem_i1,
                  OpCodeType.Ldelem_i2,
                  OpCodeType.Ldelem_i4,
                  OpCodeType.Ldelem_i8,
                  OpCodeType.Ldelem_u1,
                  OpCodeType.Ldelem_u2,
                  OpCodeType.Ldelem_u4,
                  OpCodeType.Ldelem_r4,
                  OpCodeType.Ldelem_r8,
                  OpCodeType.Ldelema)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var index = context.Stack.PopExp();
            var array = context.Stack.PopExp();
            Expression expression = Expression.ArrayIndex(array, index);

            // TODO: check array type to see if cast is redundant
            if(instruction.OpCodeType is OpCodeType.Ldelem)
            {
                var targetType = instruction.OprandAsType();

                expression = Expression.Convert(expression, targetType);
            }

            return expression;
        }
    }
}

