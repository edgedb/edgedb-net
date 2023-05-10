using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDB.Generators.SharedTests
{
    /// <summary>
    ///		A utility class for writing code.
    /// </summary>
	internal class CodeWriter
    {
        /// <summary>
        ///		The content of the code writer.
        /// </summary>
        public readonly StringBuilder Content = new StringBuilder();

        /// <summary>
        ///		Gets the indentation level of the current code writer.
        /// </summary>
        public int IndentLevel { get; private set; }

        /// <summary>
        ///		The scope tracker providing an implementation of IDisposable.
        /// </summary>
        private readonly ScopeTracker _scopeTracker; // We only need one. It can be reused.

        /// <summary>
        ///		Creates a new <see cref="CodeWriter"/>.
        /// </summary>
        public CodeWriter()
        {
            _scopeTracker = new ScopeTracker(this);
        }

        /// <summary>
        ///		Appends a string to the current code writer.
        /// </summary>
        /// <param name="line">The line to append</param>
        public void Append(string line)
            => Content.Append(line);

        /// <summary>
        ///		Appends a line to the current code writer, respecting <see cref="IndentLevel"/>
        ///		and ending in a line terminator.
        /// </summary>
        /// <param name="line">The line to append</param>
        public void AppendLine(string line)
            => Content.Append(new string(' ', IndentLevel)).AppendLine(line);

        /// <summary>
        ///		Appends an empty line to the current code writer, adding a line terminator to the end.
        /// </summary>
        public void AppendLine()
            => Content.AppendLine();

        /// <summary>
        ///		Begins a new scope with the specified line.
        /// </summary>
        /// <param name="line">The line to append.</param>
        /// <returns>An <see cref="IDisposable"/> representing the scope returned.</returns>
        public IDisposable BeginScope(string line)
        {
            AppendLine(line);
            return BeginScope();
        }

        /// <summary>
        ///		Begins a new scope, incrementing the indent level until the scope is disposed.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> representing the scope returned.</returns>
        public IDisposable BeginScope()
        {
            Content.Append(new string(' ', IndentLevel)).AppendLine("{");
            IndentLevel += 4;
            return _scopeTracker;
        }

        /// <summary>
        ///		Ends a scope, decrementing the indent level.
        /// </summary>
        public void EndScope()
        {
            IndentLevel -= 4;
            Content.Append(new string(' ', IndentLevel)).AppendLine("}");
        }

        /// <summary>
        ///		Converts the current code writer to a <see cref="string"/>.
        /// </summary>
        /// <returns>A string representing the code written to the code writer.</returns>
        public override string ToString()
            => Content.ToString();

        /// <summary>
        ///		An implementation of <see cref="IDisposable"/> responsible for scope decrementing.
        /// </summary>
        class ScopeTracker : IDisposable
        {
            /// <summary>
            ///		Gets the <see cref="CodeWriter"/> That created this <see cref="ScopeTracker"/>.
            /// </summary>
            public CodeWriter Parent { get; }

            /// <summary>
            ///		Constructs a new <see cref="ScopeTracker"/>.
            /// </summary>
            /// <param name="parent">The parent <see cref="CodeWriter"/> that created the <see cref="ScopeTracker"/>.</param>
            public ScopeTracker(CodeWriter parent)
            {
                Parent = parent;
            }

            /// <summary>
            ///		Disposes and ends the scope of this <see cref="ScopeTracker"/>.
            /// </summary>
            public void Dispose()
            {
                Parent.EndScope();
            }
        }
    }
}
