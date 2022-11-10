using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadArgumentInterpreter : BaseCILInterpreter
    {
        public LoadArgumentInterpreter()
            : base(
                  OpCodeType.Ldarga,
                  OpCodeType.Ldarga_s,
                  OpCodeType.Ldarg_0,
                  OpCodeType.Ldarg_1,
                  OpCodeType.Ldarg_2,
                  OpCodeType.Ldarg_3,
                  OpCodeType.Ldarg_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var index = instruction.OpCodeType switch
            {
                OpCodeType.Ldarg_0 => 0,
                OpCodeType.Ldarg_1 => 1,
                OpCodeType.Ldarg_2 => 2,
                OpCodeType.Ldarg_3 => 3,
                OpCodeType.Ldarg or OpCodeType.Ldarg_s or
                OpCodeType.Ldarga or OpCodeType.Ldarga_s
                    => (short)instruction.Oprand!,
                _ => throw new NotSupportedException()
            };

            return context.Parameters[index];
        }
    }
}

