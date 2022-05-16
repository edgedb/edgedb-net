using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool.Lexer
{
    internal class Token
    {
        public TokenType Type { get; set; }

        public string? Value { get; set; }

        public int StartPos { get; set; }

        public int StartLine { get; set; }

        public int EndPos { get; set; }

        public int EndLine { get; set; }
    }
}
