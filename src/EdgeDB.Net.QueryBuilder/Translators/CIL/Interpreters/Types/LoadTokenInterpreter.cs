using System;
using System.Linq.Expressions;

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
            if (!context.Reader.TryPeek(out var nextInstruction))
                throw new InvalidOperationException("No proceding instruction for typeof operation");

            if (nextInstruction.OpCodeType != OpCodeType.Call)
                throw new InvalidOperationException("Expected call after ldtoken");

            var method = nextInstruction.OprandAsMethod();

            if (method.Name != nameof(Type.GetTypeFromHandle))
                throw new NotSupportedException($"Expected {nameof(Type.GetTypeFromHandle)} but got {method.Name}");

            // consume the next instruction
            context.Reader.ReadNext(out _);

            // return the constant type
            var oprand = instruction.ParseOprand();

            return Expression.Constant(oprand);
        }
    }
}

