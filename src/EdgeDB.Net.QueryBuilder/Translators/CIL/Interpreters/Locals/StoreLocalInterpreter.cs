using System;
namespace EdgeDB.CIL.Interpreters
{
    internal class StoreLocalInterpreter : BaseCILInterpreter
    {
        public StoreLocalInterpreter()
            : base(
                  OpCodeType.Stloc,
                  OpCodeType.Stloc_1,
                  )
        {
        }
    }
}

