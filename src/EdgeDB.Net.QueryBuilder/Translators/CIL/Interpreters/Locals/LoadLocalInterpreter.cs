using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadLocalInterpreter : BaseCILInterpreter
    {
        public LoadLocalInterpreter()
            : base(
                  OpCodeType.Ldloca,
                  OpCodeType.Ldloca_s,
                  OpCodeType.Ldloc,
                  OpCodeType.Ldloc_0,
                  OpCodeType.Ldloc_1,
                  OpCodeType.Ldloc_2,
                  OpCodeType.Ldloc_3,
                  OpCodeType.Ldloc_s)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var index = instruction.OpCodeType switch
            {
                OpCodeType.Ldloc_0 => 0,
                OpCodeType.Ldloc_1 => 1,
                OpCodeType.Ldloc_2 => 2,
                OpCodeType.Ldloc_3 => 3,
                OpCodeType.Ldloc or OpCodeType.Ldloc_s or
                OpCodeType.Ldloca or OpCodeType.Ldloca_s
                    => (short)instruction.Oprand!,
                _ => throw new NotSupportedException()
            };

            return context.Locals[index];
        }
    }
}

