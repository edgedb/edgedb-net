using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class CastAndConvertInterpreter : BaseCILInterpreter
    {
        public CastAndConvertInterpreter()
            : base(
                  OpCodeType.Castclass,
                  OpCodeType.Conv_i,
                  OpCodeType.Conv_i1,
                  OpCodeType.Conv_i2,
                  OpCodeType.Conv_i4,
                  OpCodeType.Conv_i8,
                  OpCodeType.Conv_ovf_i,
                  OpCodeType.Conv_ovf_i_un,
                  OpCodeType.Conv_ovf_i1,
                  OpCodeType.Conv_ovf_i1_un,
                  OpCodeType.Conv_ovf_i2,
                  OpCodeType.Conv_ovf_i2_un,
                  OpCodeType.Conv_ovf_i4,
                  OpCodeType.Conv_ovf_i4_un,
                  OpCodeType.Conv_ovf_i8,
                  OpCodeType.Conv_ovf_i8_un,
                  OpCodeType.Conv_r4,
                  OpCodeType.Conv_r8,
                  OpCodeType.Conv_r_un,
                  OpCodeType.Conv_u,
                  OpCodeType.Conv_u1,
                  OpCodeType.Conv_u2,
                  OpCodeType.Conv_u4,
                  OpCodeType.Conv_u8)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();

            var targetType = instruction.OpCodeType switch
            {
                OpCodeType.Castclass => instruction.OprandAsType(),

                OpCodeType.Conv_i or OpCodeType.Conv_ovf_i or OpCodeType.Conv_i4
                or OpCodeType.Conv_ovf_i4 or OpCodeType.Conv_ovf_i4_un
                    => typeof(int),

                OpCodeType.Conv_i1 or OpCodeType.Conv_ovf_i1 or OpCodeType.Conv_ovf_i1_un
                    => typeof(sbyte),

                OpCodeType.Conv_i2 or OpCodeType.Conv_ovf_i2 or OpCodeType.Conv_ovf_i2_un
                    => typeof(short),

                OpCodeType.Conv_i8 or OpCodeType.Conv_ovf_i8 or OpCodeType.Conv_ovf_i8_un
                    => typeof(long),

                OpCodeType.Conv_u or OpCodeType.Conv_u4
                    => typeof(uint),

                OpCodeType.Conv_u1 => typeof(byte),
                OpCodeType.Conv_u2 => typeof(ushort),
                OpCodeType.Conv_u8 => typeof(ulong),

                OpCodeType.Conv_r4 or OpCodeType.Conv_r_un
                    => typeof(float),

                OpCodeType.Conv_r8 => typeof(double),

                _ => throw new NotImplementedException($"No converter target type could be found for {instruction.OpCodeType}")
            };

            // TODO: check if value is already type of targetType
            return Expression.Convert(value, targetType);
        }
    }
}

