using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreLocalInterpreter : BaseCILInterpreter
    {
        public StoreLocalInterpreter()
            : base(
                  OpCodeType.Stloc,
                  OpCodeType.Stloc_1,
                  OpCodeType.Stloc_2,
                  OpCodeType.Stloc_3,
                  OpCodeType.Stloc_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var index = instruction.OpCodeType switch
            {
                OpCodeType.Stloc_0 => 0,
                OpCodeType.Stloc_1 => 1,
                OpCodeType.Stloc_2 => 2,
                OpCodeType.Stloc_3 => 3,
                OpCodeType.Stloc or OpCodeType.Stloc_s
                    => (short)instruction.Oprand!,
                _ => throw new NotSupportedException($"Invalid store index {instruction.OpCodeType}")
            };

            var value = context.Stack.PopExp();

            return Expression.Assign(context.Locals[index], value);
        }
    }
}

