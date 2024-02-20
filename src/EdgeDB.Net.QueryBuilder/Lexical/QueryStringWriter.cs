// using EdgeDB.Schema;
// using System.Diagnostics;
// using System.Diagnostics.CodeAnalysis;
// using System.Text;
//
// namespace EdgeDB;
//
// [DebuggerDisplay("{Compile(null)}")]
// internal class QueryStringWriter
// {
//     private sealed class PositionedQueryStringWriter(int position, QueryStringWriter parent)
//         : QueryStringWriter(parent._builder, parent._chunks, parent.Labels)
//     {
//         protected override void OnExternalWrite(int position1, int length)
//         {
//             if (position1 <= position)
//                 position += length;
//
//             parent.OnExternalWrite(position1, length);
//         }
//
//         protected override void WriteInternal(char ch)
//         {
//             _builder.Insert(position++, ch);
//
//             UpdateLabels(position - 1, 1);
//             parent.OnExternalWrite(position - 1, 1);
//         }
//
//         protected override void WriteInternal(string str)
//         {
//             _builder.Insert(position, str);
//
//             UpdateLabels(position, str.Length);
//             parent.OnExternalWrite(position, str.Length);
//
//             position += str.Length;
//         }
//     }
//
//     internal readonly Dictionary<string, LinkedList<Marker>> Labels;
//     private readonly StringBuilder _builder;
//     private readonly SortedList<int, IntrospectionChunk> _chunks;
//
//     public bool RequiresIntrospection
//         => _chunks.Count > 0;
//
//     /// <summary>
//     ///     Gets the length in characters of this writer.
//     /// </summary>
//     public int Length
//         => _builder.Length == 0 ? 0 : _builder.Length;
//
//     /// <summary>
//     ///     Gets the index of the last character in the writer.
//     /// </summary>
//     public int TailIndex => Length - 1;
//
//     public char this[int index] => _builder[index];
//
//     public QueryStringWriter() : this(new(), new(), new())
//     {
//     }
//
//     private QueryStringWriter(StringBuilder stringBuilder, SortedList<int, IntrospectionChunk> chunks, Dictionary<string, LinkedList<Marker>> labels)
//     {
//         _builder = stringBuilder;
//         _chunks = chunks;
//         Labels = labels;
//     }
//
//     [return: NotNullIfNotNull(nameof(o))]
//     private string? Format(object? o)
//     {
//         return o switch
//         {
//             bool b => b ? "true" : "false",
//             _ => o?.ToString()
//         };
//     }
//
//     protected void UpdateLabels(int pos, int sz)
//     {
//         foreach (var label in Labels.Values.SelectMany(x => x).Where(x => x.Position <= pos))
//         {
//             label.Update(sz);
//         }
//     }
//
//     protected virtual void WriteInternal(char ch)
//     {
//         _builder.Append(ch);
//         UpdateLabels(TailIndex, 1);
//     }
//
//     protected virtual void WriteInternal(string str)
//     {
//         var pos = TailIndex;
//         _builder.Append(str);
//         UpdateLabels(pos, str.Length);
//     }
//
//     protected virtual void OnExternalWrite(int position, int length) { }
//
//     /// <summary>
//     ///     Appends a value, and returns whether or no anything was actually written.
//     /// </summary>
//     /// <param name="value">The value to append.</param>
//     /// <returns><c>true</c> if the provided values size is greater than zero, otherwise <c>false</c></returns>
//     public bool AppendIsEmpty(Action<QueryStringWriter> value)
//         => AppendIsEmpty(Val(value));
//
//     /// <summary>
//     ///     Appends a value, and returns whether or no anything was actually written.
//     /// </summary>
//     /// <param name="value">The value to append.</param>
//     /// <returns><c>true</c> if the provided values size is greater than zero, otherwise <c>false</c></returns>
//     public bool AppendIsEmpty(Value value)
//         => AppendIsEmpty(value, out _);
//
//     /// <summary>
//     ///     Appends a value, and returns whether or no anything was actually written.
//     /// </summary>
//     /// <param name="value">The value to append.</param>
//     /// <param name="size">The size of the value.</param>
//     /// <returns><c>true</c> if the provided values size is greater than zero, otherwise <c>false</c></returns>
//     public bool AppendIsEmpty(Value value, out int size)
//     {
//         var pos = TailIndex;
//         Append(value);
//         return (size = TailIndex - pos) > 0;
//     }
//
//     public QueryStringWriter Remove(int start, int count)
//         => _builder.Remove(start, count);
//
//     public Value Val(object? value)
//         => new(value);
//
//     public Value Val(WriterProxy writer)
//         => new(writer);
//
//     public QueryStringWriter GetPositionalWriter(int index = -1)
//     {
//         if (index is -1)
//             index = TailIndex;
//
//         return new PositionedQueryStringWriter(index, this);
//     }
//
//     public QueryStringWriter Label(MarkerType type, string name, Value value)
//     {
//         if (!Labels.TryGetValue(name, out var labels))
//             Labels[name] = labels = new();
//
//         var pos = TailIndex;
//         Append(value);
//         labels.AddLast(new Marker(type, this, TailIndex - pos, pos));
//         return this;
//     }
//
//     public QueryStringWriter Label(MarkerType type, string value)
//         => Label(type, value, value);
//
//     public QueryStringWriter Label(MarkerType type, string name, Action<string, QueryStringWriter> func)
//     {
//         if (!Labels.TryGetValue(name, out var labels))
//             Labels[name] = labels = new();
//
//         var pos = TailIndex;
//         func(name, this);
//         labels.AddLast(new Marker(type, this, TailIndex - pos, pos));
//         return this;
//     }
//
//     public bool TryGetLabeled(string name, [NotNullWhen(true)] out LinkedList<Marker>? markers)
//         => Labels.TryGetValue(name, out markers);
//
//     public int IndexOf(string value, bool ignoreCase = false, int startIndex = 0)
//     {
//         int index;
//         int length = value.Length;
//         int maxSearchLength = (_builder.Length - length) + 1;
//
//         if (ignoreCase)
//         {
//             for (int i = startIndex; i < maxSearchLength; ++i)
//             {
//                 if (Char.ToLower(_builder[i]) == Char.ToLower(value[0]))
//                 {
//                     index = 1;
//                     while ((index < length) && (Char.ToLower(_builder[i + index]) == Char.ToLower(value[index])))
//                         ++index;
//
//                     if (index == length)
//                         return i;
//                 }
//             }
//
//             return -1;
//         }
//
//         for (int i = startIndex; i < maxSearchLength; ++i)
//         {
//             if (_builder[i] == value[0])
//             {
//                 index = 1;
//                 while ((index < length) && (_builder[i + index] == value[index]))
//                     ++index;
//
//                 if (index == length)
//                     return i;
//             }
//         }
//
//         return -1;
//     }
//
//     public void Clear()
//     {
//         _builder.Clear();
//         _chunks.Clear();
//     }
//
//     public QueryStringWriter Insert(int index, char c)
//     {
//         _builder.Insert(index, c);
//         OnExternalWrite(index, 1);
//         return this;
//     }
//
//     public QueryStringWriter Insert(int index, string s)
//     {
//         _builder.Insert(index, s);
//         OnExternalWrite(index, s.Length);
//         return this;
//     }
//
//     public QueryStringWriter Insert(int index, Value val)
//     {
//         val.WriteAt(this, index);
//         return this;
//     }
//
//     public QueryStringWriter Insert(int index, object? o)
//     {
//         return o is null ? this : Insert(index, Format(o));
//     }
//
//     public QueryStringWriter Insert(int index, QueryStringWriter writer)
//     {
//         var offset = _builder.Length;
//         _builder.Insert(offset, writer._builder);
//         OnExternalWrite(offset, writer._builder.Length);
//
//         foreach (var chunk in writer._chunks)
//         {
//             _chunks.Add(chunk.Key + offset, chunk.Value);
//         }
//
//         return this;
//     }
//
//     public QueryStringWriter AppendIf(Func<bool> condition, Value value)
//     {
//         if (condition())
//             value.WriteTo(this);
//
//         return this;
//     }
//
//     public QueryStringWriter Append(char c)
//     {
//         WriteInternal(c);
//         return this;
//     }
//
//     public QueryStringWriter Append(string? s)
//     {
//         if (s is null)
//             return this;
//
//         WriteInternal(s);
//         return this;
//     }
//
//     public QueryStringWriter Append(object? o)
//     {
//         if (o is null)
//             return this;
//
//         Append(Format(o));
//
//         return this;
//     }
//
//     public QueryStringWriter Append(WriterProxy value)
//         => Append(new Value(value));
//
//     public QueryStringWriter Append(Value value)
//     {
//         value.WriteTo(this);
//         return this;
//     }
//
//     public QueryStringWriter Append(QueryStringWriter writer)
//     {
//         var offset = _builder.Length;
//         _builder.Append(writer._builder);
//         OnExternalWrite(offset, writer._builder.Length);
//
//         foreach (var chunk in writer._chunks)
//         {
//             _chunks.Add(chunk.Key + offset, chunk.Value);
//         }
//
//         return this;
//     }
//
//     public QueryStringWriter AppendIntrospected(Action<SchemaInfo, QueryStringWriter> func)
//     {
//         _chunks.Add(_builder.Length, new IntrospectionChunk(_builder.Length, func));
//         return this;
//     }
//
//     public QueryStringWriter SingleQuoted(Value value)
//         => Append('\'').Append(value).Append('\'');
//
//     public QueryStringWriter Assignment(object name, Value value)
//         => Append(name).Append(" := ").Append(value);
//
//     public QueryStringWriter Assignment(object name, WriterProxy value)
//         => Append(name).Append(" := ").Append(value);
//
//     public QueryStringWriter QueryArgument(object? type, Value name)
//         => QueryArgument(new Value(type), name);
//
//     public QueryStringWriter QueryArgument(Value type, Value name)
//         => TypeCast(type).Append('$').Append(name);
//
//     public QueryStringWriter TypeCast(object? type)
//         => Append('<').Append(Format(type)).Append('>');
//
//     public QueryStringWriter TypeCast(Value type)
//         => Append('<').Append(type).Append('>');
//
//     public QueryStringWriter Wrapped(WriterProxy func, string chars = "()")
//         => Wrapped(new Value(func), chars);
//
//     public QueryStringWriter Wrapped(Value value, string chars = "()")
//     {
//         if (chars.Length is not 2)
//             throw new ArgumentOutOfRangeException(nameof(chars), "must contain 2 characters");
//
//         return Append(chars[0]).Append(value).Append(chars[1]);
//     }
//
//     public QueryStringWriter Shape<T>(params T[] elements)
//         where T : notnull
//     {
//         Label(MarkerType.Shape, $"shape_{elements.GetHashCode()}", (name, writer) =>
//         {
//             writer.Append('{');
//
//             for (var i = 0; i != elements.Length;)
//             {
//                 writer.Append(elements[i++]);
//
//                 if (i != elements.Length)
//                     writer.Append(", ");
//             }
//
//             writer.Append('}');
//         });
//
//         return this;
//     }
//
//     public QueryStringWriter Shape<T>(IEnumerable<T> elements, Action<QueryStringWriter, T> writer,
//         string shapeChars = "{}")
//     {
//         if (shapeChars.Length is not 2)
//             throw new ArgumentOutOfRangeException(nameof(shapeChars), "must contain 2 characters");
//
//         Label(MarkerType.Shape, $"shape_{elements.GetHashCode()}", (name, wt) =>
//         {
//             wt.Append(shapeChars[0]);
//
//             using var enumerator = elements.GetEnumerator();
//
//             enumerator.MoveNext();
//
//             loop:
//
//             // check for empty entries
//             var i = TailIndex;
//             writer(wt, enumerator.Current);
//
//             // if nothing was written, continue the iteration without adding a delimiter
//             if (i == TailIndex)
//             {
//                 enumerator.MoveNext();
//                 goto loop;
//             }
//
//             if (enumerator.MoveNext())
//             {
//                 wt.Append(", ");
//                 goto loop;
//             }
//
//             wt.Append(shapeChars[1]);
//         });
//
//         return this;
//     }
//
//     public QueryStringWriter Shape<T>(IEnumerable<T> elements, string shapeChars = "{}")
//         => Shape(elements, (writer, value) => writer.Append(value), shapeChars);
//
//     public readonly struct FunctionArg
//     {
//         public readonly Value Value;
//         public readonly string? Named;
//
//         public FunctionArg(Value value, string? named = null)
//         {
//             Value = value;
//             Named = named;
//         }
//
//         public static implicit operator FunctionArg(Value value) => new(value);
//         public static implicit operator FunctionArg(string? str) => new(str);
//         public static implicit operator FunctionArg(char ch) => new(ch);
//         public static implicit operator FunctionArg(WriterProxy writerProxy) => new(writerProxy);
//     }
//
//     public QueryStringWriter Function(object function, params FunctionArg[] args)
//     {
//         Label(MarkerType.Function, $"func_{function}_{args.GetHashCode()}", (name, writer) =>
//         {
//             writer.Append(function).Append('(');
//
//             for (var i = 0; i < args.Length;)
//             {
//                 var arg = args[i++];
//
//                 if(AppendIsEmpty(arg.Value, out var size))
//                     continue;
//
//                 // append the named part if its specified
//                 if (arg.Named is not null)
//                     writer.GetPositionalWriter(TailIndex - size + 1)
//                         .Append(arg.Named)
//                         .Append(" := ");
//
//                 if (i != args.Length)
//                     writer.Append(", ");
//             }
//
//             writer.Append(')');
//         });
//
//         return this;
//     }
//
//     public StringBuilder Compile(SchemaInfo? info = null)
//     {
//         if (RequiresIntrospection && info is null)
//             throw new InvalidOperationException("Introspection is required to compile this query");
//
//         int offset = 0;
//         var sb = new QueryStringWriter();
//
//         foreach (var chunk in _chunks)
//         {
//             chunk.Value.Compile(info!, sb);
//             _builder.Insert(offset += chunk.Key, sb);
//             sb.Clear();
//         }
//
//         return _builder;
//     }
//
//     private sealed class IntrospectionChunk(int index, Action<SchemaInfo, QueryStringWriter> compiler)
//     {
//         public int Index { get; } = index;
//
//         public void Compile(SchemaInfo info, QueryStringWriter builder)
//             => compiler(info, builder);
//     }
//
//     public static implicit operator QueryStringWriter(StringBuilder stringBuilder) => new(stringBuilder, new(), new());
// }
