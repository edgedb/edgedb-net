using System;
using System.Linq.Expressions;

namespace EdgeDB.CIL.Interpreters
{
    internal class BranchIfFalseInterpreter : BaseConditionalBranchInterpreter
    {
        public BranchIfFalseInterpreter()
            : base(
                  OpCodeType.Brfalse,
                  OpCodeType.Brfalse_s,
                  OpCodeType.Brtrue,
                  OpCodeType.Brtrue_s)
        {
        }

        protected override Expression GetCondition(CILInterpreterContext context)
            => context.Stack.PopExp();
    }
}

