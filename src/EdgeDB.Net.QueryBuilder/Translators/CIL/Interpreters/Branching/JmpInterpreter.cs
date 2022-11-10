using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class JmpInterpreter : BaseCILInterpreter
    {
        public JmpInterpreter()
            : base(OpCodeType.Jmp)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var targetMethod = instruction.OprandAsMethod();

            if(targetMethod is not MethodInfo targetMethodInfo)
                throw new NotSupportedException($"Expected a method info but got {targetMethod.GetType()}");

            // TODO: instance?

            return Expression.Call(null, targetMethodInfo, context.Parameters);
        }
    }
}

