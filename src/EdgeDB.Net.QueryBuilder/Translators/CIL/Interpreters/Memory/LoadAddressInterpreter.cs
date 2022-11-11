using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadAddressInterpreter : BaseCILInterpreter
    {
        public LoadAddressInterpreter()
            : base(
                  OpCodeType.Ldind_i,
                  OpCodeType.Ldind_i1,
                  OpCodeType.Ldind_i2,
                  OpCodeType.Ldind_i4,
                  OpCodeType.Ldind_i8,
                  OpCodeType.Ldind_r4,
                  OpCodeType.Ldind_r8,
                  OpCodeType.Ldind_ref,
                  OpCodeType.Ldind_u1,
                  OpCodeType.Ldind_u2,
                  OpCodeType.Ldind_u4)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // Since any time we incounter a store address etc
            // we directly store the value that address points to,
            // any load adress instructions can act as noop.

            return Expression.Empty();
        }
    }
}

