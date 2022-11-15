using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class StoreFieldInterpreter : BaseCILInterpreter
    {
        public StoreFieldInterpreter()
            : base(OpCodeType.Stsfld, OpCodeType.Stfld)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var field = instruction.OprandAsField();

            var value = context.Stack.PopExp();

            var instance = instruction.OpCodeType is OpCodeType.Stfld
                ? context.Stack.PopExp()
                : null;

            Refine(ref value, context, field.FieldType);

            // TODO: is this right syntax?
            return Expression.Assign(
                Expression.Field(instance, field),
                value);
        }
    }
}

