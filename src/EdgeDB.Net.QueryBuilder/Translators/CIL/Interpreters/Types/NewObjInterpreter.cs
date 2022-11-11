using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EdgeDB.CIL.Interpreters
{
    internal class NewObjInterpreter : BaseCILInterpreter
    {
        public NewObjInterpreter()
            : base(OpCodeType.Newobj)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            if (instruction.OprandAsMethod() is not ConstructorInfo ctor)
                throw new NotSupportedException("Oprand must be a constructor");

            var arguments = new Expression[ctor.GetParameters().Length];

            for(int i = arguments.Length - 1; i >= 0; i--)
                arguments[i] = context.Stack.PopExp();

            return Expression.New(ctor, arguments);
        }
    }
}

