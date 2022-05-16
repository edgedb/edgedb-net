using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDB.QueryBuilder.OperatorGenerator
{
	internal class CodeWriter
	{
		public readonly StringBuilder Content = new();
		public int IndentLevel { get; private set; }
		
		private readonly ScopeTracker _scopeTracker; //We only need one. It can be reused.

		public CodeWriter()
		{
			_scopeTracker = new(this); //We only need one. It can be reused.
		}

		public void Append(string line) 
			=> Content.Append(line);

		public void AppendLine(string line) 
			=> Content.Append(new string(' ', IndentLevel)).AppendLine(line);

		public void AppendLine() 
			=> Content.AppendLine();

		public IDisposable BeginScope(string line)
		{
			AppendLine(line);
			return BeginScope();
		}

		public IDisposable BeginScope()
		{
			Content.Append(new string(' ', IndentLevel)).AppendLine("{");
			IndentLevel += 4;
			return _scopeTracker;
		}

		public void EndLine() 
			=> Content.AppendLine();

		public void EndScope()
		{
			IndentLevel -= 4;
			Content.Append(new string(' ', IndentLevel)).AppendLine("}");
		}

		public void StartLine() 
			=> Content.Append(new string(' ', IndentLevel));

		public override string ToString() 
			=> Content.ToString();

		class ScopeTracker : IDisposable
		{
			public ScopeTracker(CodeWriter parent)
			{
				Parent = parent;
			}
			public CodeWriter Parent { get; }

			public void Dispose()
			{
				Parent.EndScope();
			}
		}
	}
}
