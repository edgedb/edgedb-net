using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CIL
{
    internal readonly struct Label
    {
        public readonly int Offset;

        public Label(int offset)
        {
            Offset = offset;
        }

        public override string ToString()
        {
            return $"IL_{Offset:x4}";
        }
    }
}
