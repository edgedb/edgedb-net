using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace EdgeDB.CIL.Interpreters
{
    internal class SizeofInterpreter : BaseCILInterpreter
    {
        public SizeofInterpreter()
            : base(OpCodeType.Sizeof)
        {
        }

        public override Expression Interpret(Instruction instruction, CILInterpreterContext context)
        {
            var type = instruction.OprandAsType();
            return Expression.Constant(Marshal.SizeOf(type));
        }
    }
}

