using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadNumberInterpreter : BaseCILInterpreter
    {
        public LoadNumberInterpreter()
            : base(
                  // int32
                  OpCodeType.Ldc_i4_m1,
                  OpCodeType.Ldc_i4_0,
                  OpCodeType.Ldc_i4_1,
                  OpCodeType.Ldc_i4_2,
                  OpCodeType.Ldc_i4_3,
                  OpCodeType.Ldc_i4_4,
                  OpCodeType.Ldc_i4_5,
                  OpCodeType.Ldc_i4_6,
                  OpCodeType.Ldc_i4_7,
                  OpCodeType.Ldc_i4_8,
                  OpCodeType.Ldc_i4,
                  //int64
                  OpCodeType.Ldc_i8,
                  // double
                  OpCodeType.Ldc_r8,
                  // float
                  OpCodeType.Ldc_r4)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            object value = instruction.OpCodeType switch
            {
                OpCodeType.Ldc_i4_0 => 0,
                OpCodeType.Ldc_i4_1 => 1,
                OpCodeType.Ldc_i4_2 => 2,
                OpCodeType.Ldc_i4_3 => 3,
                OpCodeType.Ldc_i4_4 => 4,
                OpCodeType.Ldc_i4_5 => 5,
                OpCodeType.Ldc_i4_6 => 6,
                OpCodeType.Ldc_i4_7 => 7,
                OpCodeType.Ldc_i4_8 => 8,
                OpCodeType.Ldc_i4_m1 => -1,
                _ => instruction.ParseOprand()!
            };

            return Expression.Constant(value);
        }
    }
}

