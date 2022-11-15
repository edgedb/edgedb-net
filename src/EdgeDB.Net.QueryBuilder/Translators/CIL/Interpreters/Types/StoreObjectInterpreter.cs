using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreObjectInterpreter : BaseCILInterpreter
    {
        public StoreObjectInterpreter()
            : base(OpCodeType.Stobj)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var value = context.Stack.PopExp();
            var target = context.Stack.PopExp();

            Refine(ref value, context, target.Type);

            return Expression.Assign(target, value);
        }
    }
}

