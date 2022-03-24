using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Test
{
    public enum Severity
    {
        Debug,
        Verbose,
        Info,
        Trace,
        Log,
        Warning,
        Error,
        Critical,
    }

    public enum StreamType
    {
        StandardIn,
        StandardOut,
        StandardError
    }

    public class Logger : ILogger
    {
        private string _owner;
        private Severity[] _severities;

        private static ConcurrentQueue<LogMessage> _queue = new ConcurrentQueue<LogMessage>();
        private static Dictionary<string, Func<string, Task>> _commands = new Dictionary<string, Func<string, Task>>();
        private static object _lock = new object();
        private static TaskCompletionSource _taskSource;
        private static List<(StreamType Type, Stream Stream)> _streams;
        private static List<StreamWriter> _stdOut => _streams.Where(x => x.Type == StreamType.StandardOut).Select(x => new StreamWriter(x.Stream)).ToList();
        private static List<StreamWriter> _stdErr => _streams.Where(x => x.Type == StreamType.StandardError).Select(x => new StreamWriter(x.Stream)).ToList();
        private static List<StreamReader> _stdIn => _streams.Where(x => x.Type == StreamType.StandardIn).Select(x => new StreamReader(x.Stream)).ToList();

        private static Dictionary<Severity, ConsoleColor> _severityColors = new Dictionary<Severity, ConsoleColor>()
    {
        { Severity.Log, ConsoleColor.Green },
        { Severity.Error, ConsoleColor.Red },
        { Severity.Warning, ConsoleColor.Yellow },
        { Severity.Critical, ConsoleColor.DarkRed },
        { Severity.Debug, ConsoleColor.Gray },
        { Severity.Verbose, ConsoleColor.DarkCyan },
        { Severity.Info, ConsoleColor.White },
        { Severity.Trace, ConsoleColor.Yellow },

    };
        public void Trace(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Trace);
        public void Log(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Log);
        public void Warn(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Warning);
        public void Error(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Error);
        public void Critical(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Critical);
        public void Debug(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Debug);
        public void Info(string content, Severity severity = Severity.Log, Exception? exception = null, bool stdErr = false)
            => Write(content, exception, stdErr, severity, Severity.Info);
        public void Write(string conent, Exception? exception, bool stdErr = false, params Severity[] severity)
            => Write(conent, severity, exception, stdErr);
        public void Write(string content, IEnumerable<Severity> severity, Exception? exception = null, bool stdErr = false)
        {
            if (!_severities.Contains(severity.First()))
                return;

            var type = (stdErr || exception != null) ? StreamType.StandardError : StreamType.StandardOut;

            if (exception != null)
                content += $" Exception: {exception}";

            var msg = CreateLogMessage(severity, content, type);

            _queue.Enqueue(msg);
            _taskSource.TrySetResult();
        }

        private Logger(string caller, Severity[] sevs)
        {
            _owner = caller;
            _severities = sevs;
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
            => WriteToStreamsAsync(msg, _stdOut);

        private static Task WriteStdErrAsync(string msg)
            => WriteToStreamsAsync(msg, _stdErr);

        private static async Task WriteToStreamsAsync(string msg, IEnumerable<StreamWriter> writers)
        {
            foreach (var stream in writers)
            {
                await stream.WriteAsync(msg + "\n");
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public static Logger GetLogger<TType>(params Severity[] sevs)
            => GetLogger(typeof(TType), sevs);
        public static Logger GetLogger(Type t, params Severity[] sevs)
        {
            return new Logger($"{t.Assembly.GetName().Name}:{t.Name}", sevs);
        }

        public static void AddStream(Stream stream, StreamType type)
        {
            _streams.Add((type, stream));
        }

        public static void RegisterCommand(string commandName, Func<string, Task> commandResult)
        {
            _commands.TryAdd(commandName, commandResult);
        }

        private LogMessage CreateLogMessage(IEnumerable<Severity> severities, string message, StreamType type)
        {
            var enumsWithColors = "";
            foreach (var item in severities)
            {
                if (enumsWithColors == "")
                    enumsWithColors = $"<{(int)_severityColors[item]}>{item}</{(int)_severityColors[item]}>";
                else
                    enumsWithColors += $" -> <{(int)_severityColors[item]}>{item}</{(int)_severityColors[item]}>";
            }

            var items = ProcessColors($"\u001b[38;5;249m{DateTime.UtcNow.ToString("O")} <Green>{_owner}</Green> " + $"\u001b[1m[{enumsWithColors}]\u001b[0m - \u001b[37;1m{message}");

            string content = $"{string.Join("", items.Select(item => $"{ConsoleColorToANSI(item.color)}{item.value}\u001b[0m"))}";

            return new LogMessage()
            {
                Caller = _owner,
                Content = content,
                Type = type
            };
        }

        private static Regex ColorRegex = new Regex(@"<(.*)>(.*?)<\/\1>");
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
            if (Enum.TryParse(typeof(ConsoleColor), tag, true, out var res))
            {
                return (ConsoleColor?)res;
            }
            else if (int.TryParse(tag, out var r))
            {
                return (ConsoleColor)r;
            }
            else return null;
        }

        private static string ConsoleColorToANSI(ConsoleColor color)
        {
            int ansiConverter(ConsoleColor c)
            {
                switch (c)
                {
                    case ConsoleColor.Black:
                        return 0;
                    case ConsoleColor.DarkRed:
                        return 1;
                    case ConsoleColor.DarkGreen:
                        return 2;
                    case ConsoleColor.DarkYellow:
                        return 3;
                    case ConsoleColor.DarkBlue:
                        return 4;
                    case ConsoleColor.DarkMagenta:
                        return 5;
                    case ConsoleColor.DarkCyan:
                        return 6;
                    case ConsoleColor.Gray:
                        return 7;
                    case ConsoleColor.DarkGray:
                        return 8;
                    case ConsoleColor.Red:
                        return 9;
                    case ConsoleColor.Green:
                        return 10;
                    case ConsoleColor.Yellow:
                        return 11;
                    case ConsoleColor.Blue:
                        return 12;
                    case ConsoleColor.Magenta:
                        return 13;
                    case ConsoleColor.Cyan:
                        return 14;
                    case ConsoleColor.White:
                        return 15;
                    default:
                        return (int)c;
                }
            }

            return $"\u001b[38;5;{ansiConverter(color)}m";
        }

        public static string BuildColoredString(object? s, ConsoleColor color)
            => BuildColoredString(s?.ToString(), color);
        public static string BuildColoredString(string? s, ConsoleColor color)
        {
            return $"<{color}>{s}</{color}>";
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state == null)
                return;
            var lvl = logLevel switch
            {
                LogLevel.Debug => Severity.Debug,
                LogLevel.Critical => Severity.Critical,
                LogLevel.Error => Severity.Error,
                LogLevel.Information => Severity.Info,
                LogLevel.None => Severity.Log,
                LogLevel.Trace => Severity.Trace,
                LogLevel.Warning => Severity.Warning,
                _ => Severity.Log
            };

            Write(state.ToString()!, exception, severity: lvl);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        private struct LogMessage
        {
            public string Caller { get; set; }
            public string Content { get; set; }
            public StreamType Type { get; set; }
        }
    }
}
