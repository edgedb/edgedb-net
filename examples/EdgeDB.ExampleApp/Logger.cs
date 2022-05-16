using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace EdgeDB.ExampleApp
{
    public enum LogPostfix
    {
        Debug,
        Verbose,
        Info,
        Trace,
        Log,
        Warning,
        Error,
        Critical,

        // sources
        Examples,
        EdgeDB
    }

    public enum StreamType
    {
        StandardIn,
        StandardOut,
        StandardError
    }

    public class Logger : ILogger
    {
        private readonly string _owner;
        private readonly LogPostfix[] _postFixs;

        private static readonly ConcurrentQueue<LogMessage> _queue = new();
        private static readonly Dictionary<string, Func<string, Task>> _commands = new();
        private static readonly object _lock = new();
        private static TaskCompletionSource _taskSource;
        private static readonly List<(StreamType Type, Stream Stream)> _streams;
        private static List<StreamWriter> StdOut => _streams.Where(x => x.Type == StreamType.StandardOut).Select(x => new StreamWriter(x.Stream)).ToList();
        private static List<StreamWriter> StdErr => _streams.Where(x => x.Type == StreamType.StandardError).Select(x => new StreamWriter(x.Stream)).ToList();

        private static readonly Dictionary<LogPostfix, ConsoleColor> _postfixColors = new()
        {
            { LogPostfix.Log, ConsoleColor.Green },
            { LogPostfix.Error, ConsoleColor.Red },
            { LogPostfix.Warning, ConsoleColor.Yellow },
            { LogPostfix.Critical, ConsoleColor.DarkRed },
            { LogPostfix.Debug, ConsoleColor.Gray },
            { LogPostfix.Verbose, ConsoleColor.DarkCyan },
            { LogPostfix.Info, ConsoleColor.White },
            { LogPostfix.Trace, ConsoleColor.Yellow },
            { LogPostfix.Examples, ConsoleColor.Yellow },
            { LogPostfix.EdgeDB, ConsoleColor.Cyan }

        };
        public void Trace(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Trace);

        public void Log(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Log);

        public void Warn(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Warning);

        public void Error(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Error);

        public void Critical(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Critical);

        public void Debug(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Debug);

        public void Info(string content, LogPostfix postfix = LogPostfix.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, postfix, LogPostfix.Info);

        public void Write(string conent, Exception? exception, bool stdErr = false, params LogPostfix[] postfix)
            => Write(conent, postfix, exception, stdErr);

        public void Write(string content, IEnumerable<LogPostfix> postfix, Exception? exception = null, bool stdErr = false)
        {
            if (_postFixs.Length > 0 && !_postFixs.Contains(postfix.First()))
                return;

            var type = (stdErr || exception != null) ? StreamType.StandardError : StreamType.StandardOut;

            if (exception != null)
                content += $" Exception: {exception}";

            var msg = CreateLogMessage(postfix, content, type);

            _queue.Enqueue(msg);
            _taskSource.TrySetResult();
        }

        private Logger(string caller, LogPostfix[] postfixs)
        {
            _owner = caller;
            _postFixs = postfixs;
        }

        static Logger()
        {
            _streams = new List<(StreamType Type, Stream Stream)>();
            _taskSource = new TaskCompletionSource();
            _ = Task.Run(async () => await QueueRunnerAsync());
        }

        private static async Task QueueRunnerAsync()
        {
            while (!Environment.HasShutdownStarted)
            {
                bool wait = false;

                lock (_lock)
                {
                    wait = _queue.IsEmpty;
                }

                if (wait)
                    await _taskSource.Task.ConfigureAwait(false);

                lock (_lock)
                {
                    _taskSource = new TaskCompletionSource();
                }

                while (_queue.TryDequeue(out var msg))
                {
                    if (msg.Type == StreamType.StandardError)
                        await WriteStdErrAsync(msg.Content).ConfigureAwait(false);
                    else if (msg.Type == StreamType.StandardOut)
                        await WriteStdOutAsync(msg.Content).ConfigureAwait(false);
                }
            }
        }

        private static Task WriteStdOutAsync(string msg)
            => WriteToStreamsAsync(msg, StdOut);

        private static Task WriteStdErrAsync(string msg)
            => WriteToStreamsAsync(msg, StdErr);

        private static async Task WriteToStreamsAsync(string msg, IEnumerable<StreamWriter> writers)
        {
            foreach (var stream in writers)
            {
                await stream.WriteAsync(msg + "\n");
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public static Logger GetLogger<TType>(params LogPostfix[] postfixs)
            => GetLogger(typeof(TType), postfixs);

        public static Logger GetLogger(Type t, params LogPostfix[] postfixs)
            => new($"{t.Assembly.GetName().Name}:{t.Name}", postfixs);

        public static void AddStream(Stream stream, StreamType type) 
            => _streams.Add((type, stream));

        public static void RegisterCommand(string commandName, Func<string, Task> commandResult) 
            => _commands.TryAdd(commandName, commandResult);

        private LogMessage CreateLogMessage(IEnumerable<LogPostfix> severities, string message, StreamType type)
        {
            var enumsWithColors = "";
            foreach (var item in severities)
            {
                if (enumsWithColors == "")
                    enumsWithColors = $"<{(int)_postfixColors[item]}>{item}</{(int)_postfixColors[item]}>";
                else
                    enumsWithColors += $" -> <{(int)_postfixColors[item]}>{item}</{(int)_postfixColors[item]}>";
            }

            var items = ProcessColors($"\u001b[38;5;249m{DateTime.UtcNow:O} <Green>{_owner}</Green> " + $"\u001b[1m[{enumsWithColors}]\u001b[0m - \u001b[37;1m{message}");

            string content = $"{string.Join("", items.Select(item => $"{ConsoleColorToANSI(item.color)}{item.value}\u001b[0m"))}";

            return new LogMessage()
            {
                Caller = _owner,
                Content = content,
                Type = type
            };
        }

        private static readonly Regex ColorRegex = new(@"<(.*)>(.*?)<\/\1>");
        private static List<(ConsoleColor color, string value)> ProcessColors(string input)
        {
            var returnData = new List<(ConsoleColor color, string value)>();

            var mtch = ColorRegex.Matches(input);

            if (mtch.Count == 0)
            {
                returnData.Add((ConsoleColor.White, input));
                return returnData;
            }

            for (int i = 0; i != mtch.Count; i++)
            {
                var match = mtch[i];
                var color = GetColor(match.Groups[1].Value) ?? ConsoleColor.White;

                if (i == 0)
                {
                    if (match.Index != 0)
                    {
                        returnData.Add((ConsoleColor.White, new string(input.Take(match.Index).ToArray())));
                    }
                    returnData.Add((color, match.Groups[2].Value));
                }
                else
                {
                    var previousMatch = mtch[i - 1];
                    var start = previousMatch.Index + previousMatch.Length;
                    var end = match.Index;

                    returnData.Add((ConsoleColor.White, new string(input.Skip(start).Take(end - start).ToArray())));

                    returnData.Add((color, match.Groups[2].Value));
                }

                if (i + 1 == mtch.Count)
                {
                    // check remainder
                    if (match.Index + match.Length < input.Length)
                    {
                        returnData.Add((ConsoleColor.White, new string(input.Skip(match.Index + match.Length).ToArray())));
                    }
                }
            }

            return returnData;
        }

        private static ConsoleColor? GetColor(string tag)
        {
            return Enum.TryParse(typeof(ConsoleColor), tag, true, out var res)
                ? (ConsoleColor?)res
                : int.TryParse(tag, out var r) ? (ConsoleColor)r : null;
        }

        private static string ConsoleColorToANSI(ConsoleColor color)
        {
            static int ansiConverter(ConsoleColor c)
            {
                return c switch
                {
                    ConsoleColor.Black => 0,
                    ConsoleColor.DarkRed => 1,
                    ConsoleColor.DarkGreen => 2,
                    ConsoleColor.DarkYellow => 3,
                    ConsoleColor.DarkBlue => 4,
                    ConsoleColor.DarkMagenta => 5,
                    ConsoleColor.DarkCyan => 6,
                    ConsoleColor.Gray => 7,
                    ConsoleColor.DarkGray => 8,
                    ConsoleColor.Red => 9,
                    ConsoleColor.Green => 10,
                    ConsoleColor.Yellow => 11,
                    ConsoleColor.Blue => 12,
                    ConsoleColor.Magenta => 13,
                    ConsoleColor.Cyan => 14,
                    ConsoleColor.White => 15,
                    _ => (int)c,
                };
            }

            return $"\u001b[38;5;{ansiConverter(color)}m";
        }

        public static string BuildColoredString(object? s, ConsoleColor color)
            => BuildColoredString(s?.ToString(), color);

        public static string BuildColoredString(string? s, ConsoleColor color) 
            => $"<{color}>{s}</{color}>";

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state is null)
                return;

            var lvl = logLevel switch
            {
                LogLevel.Debug => LogPostfix.Debug,
                LogLevel.Critical => LogPostfix.Critical,
                LogLevel.Error => LogPostfix.Error,
                LogLevel.Information => LogPostfix.Info,
                LogLevel.None => LogPostfix.Log,
                LogLevel.Trace => LogPostfix.Trace,
                LogLevel.Warning => LogPostfix.Warning,
                _ => LogPostfix.Log
            };

            Write(state.ToString()!, exception, postfix: lvl);
        }

        public bool IsEnabled(LogLevel logLevel) 
            => true;

        public IDisposable BeginScope<TState>(TState state) 
            => null!;

        private struct LogMessage
        {
            public string Caller { get; set; }

            public string Content { get; set; }

            public StreamType Type { get; set; }
        }
    }
}
