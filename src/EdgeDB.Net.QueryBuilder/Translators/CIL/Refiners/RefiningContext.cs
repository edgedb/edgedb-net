using EdgeDB.CIL.Interpreters;
using System;
using System.Reflection;

namespace EdgeDB.CIL
{
    internal sealed class RefiningContext
    {
        public Type? TargetType { get; set; }

        public CILInterpreterContext CILContext { get; set; }

        public Module Module
            => CILContext.Reader.MethodBase.Module;

        public InterpreterStack Stack
            => CILContext.Stack;

        public RefiningContext(CILInterpreterContext context)
        {
            CILContext = context;
        }

        public RefiningContext Enter(Action<RefiningContext> func)
        {
            var copy = (RefiningContext)MemberwiseClone();
            func(copy);
            return copy;
        }
    }
}

