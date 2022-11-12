using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreAddressInterpreter : BaseCILInterpreter
    {
        public StoreAddressInterpreter()
            : base(
                  OpCodeType.Stind_i,
                  OpCodeType.Stind_i1,
                  OpCodeType.Stind_i2,
                  OpCodeType.Stind_i4,
                  OpCodeType.Stind_i8,
                  OpCodeType.Stind_r4,
                  OpCodeType.Stind_r8,
                  OpCodeType.Stind_ref)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();
            var address = context.Stack.PopExp();

            // TODO: ensure valid typing for value -> address

            return Expression.Assign(address, value);
        }
    }
}

