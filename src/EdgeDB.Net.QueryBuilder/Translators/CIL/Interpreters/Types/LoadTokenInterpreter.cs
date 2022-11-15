using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace EdgeDB.CIL.Interpreters
{
    internal class LoadTokenInterpreter : BaseCILInterpreter
    {
        public LoadTokenInterpreter()
            : base(OpCodeType.Ldtoken)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            // TODO: loadtoken is used for arrays!

            // if the current call is load token followed by a call to
            // Type.GetTypeFromHandle(System.RuntimeTypeHandle valuetype)
            // we can push the type of the loaded token as a constant
            var handle = instruction.GetMetadataHandle();
            
            context.Stack.Push(Expression.Constant(handle, typeof(EntityHandle)));

            return Expression.Empty();
        }
    }
}

