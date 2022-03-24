using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool
{
    internal interface ICommand
    {
        void Execute();
    }
}
