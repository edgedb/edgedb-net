using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DotnetTool.Lexer
{
    internal enum TokenType
    {
        BeginBrace,
        EndBrace,
        Required,
        Property,
        Constraint,
        TypeArrow,
        Multi,
        Link,
        Abstract,
        Semicolon,
        Assignment,
        Single,
        On,
        Extending,
        Comma,
        BeginParenthesis,
        EndParenthesis,
        Annotation,
        Module,
        Type,
        Index,
        Scalar,
        Function,

        Identifier,
        EndOfFile
    }
}
