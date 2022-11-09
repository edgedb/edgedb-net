using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BinaryBitwiseInterpreter : BaseCILInterpreter
    {
        public BinaryBitwiseInterpreter()
            : base(
                OpCodeType.And,
                OpCodeType.Or,
                OpCodeType.Xor,
                OpCodeType.Shl,
                OpCodeType.Shr,
                OpCodeType.Shr_un)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var right = context.Stack.Pop();
            var left = context.Stack.Pop();

            return instruction.OpCodeType switch
            {
                OpCodeType.And
                    => Expression.And(left, right),
                OpCodeType.Or
                    => Expression.Or(left, right),
                OpCodeType.Xor
                    => Expression.ExclusiveOr(left, right),
                OpCodeType.Shl
                    => Expression.LeftShift(left, right),
                OpCodeType.Shr or OpCodeType.Shr_un
                    => Expression.RightShift(left, right),

                _ => throw new Exception($"Unkown binary bitwise operation {instruction.OpCodeType}")
            };
        }
    }
}

