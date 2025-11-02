using System;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyExtensions.AspNetCore.Formatters
{
    /// <summary>
    /// A simple console formatter that outputs log messages in a minimal format with optional color coding.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SimpleConsoleFormatter"/> class with the name "minimal".
    /// </remarks>
    public class SimpleConsoleFormatter(IOptionsMonitor<SimpleConsoleFormatterOptions> options) 
        : ConsoleFormatter(FormatterName)
    {
        /// <summary>
        /// The name of the formatter.
        /// </summary>
        public const string FormatterName = nameof(SimpleConsoleFormatter);

        private readonly IOptionsMonitor<SimpleConsoleFormatterOptions> _options = 
            options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Writes a log entry to the specified <see cref="TextWriter"/> in a simple format.
        /// </summary>
        /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
        /// <param name="logEntry">The log entry to write.</param>
        /// <param name="scopeProvider">The scope provider for the log entry.</param>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write the log entry to.</param>
        public override void Write<TState>(in LogEntry<TState> logEntry,
            IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            var level = logEntry.LogLevel;
            string category = logEntry.Category ?? string.Empty;
            string displayCategory = ShortCategory(category);

            var opts = _options.CurrentValue;
            bool colorsDisabled = opts.ColorBehavior == LoggerColorBehavior.Disabled;
            bool toConsole = !Console.IsOutputRedirected;
            bool useAnsi = !colorsDisabled && !toConsole;

            // First line: header + message
            WriteHeader(textWriter, ts, level, displayCategory, useAnsi);
            string msg = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception) ?? string.Empty;
            textWriter.Write(msg);

            // Do NOT render structured state inline for non-exception logs to avoid duplicating template args
            // (values are already substituted by the formatter into the message above).

            textWriter.WriteLine();

            // Exception: extra lines with [Ex]
            if (logEntry.Exception != null)
            {
                WriteExceptionLines(textWriter, ts, level, displayCategory, logEntry.Exception, useAnsi);
            }
        }

        private static void WriteHeader(TextWriter w, string ts, LogLevel level, string displayCategory, bool useAnsi)
        {
            w.Write('[');
            w.Write(ts);
            w.Write(' ');
            if (useAnsi)
            {
                w.Write(AnsiForLevel(level));
                w.Write(ShortLevel(level));
                w.Write(AnsiReset);
            }
            else
            {
                w.Write(ShortLevel(level));
            }
            w.Write("] ");

            w.Write('[');
            if (useAnsi)
            {
                w.Write(AnsiCyan);
                w.Write(displayCategory);
                w.Write(AnsiReset);
            }
            else
            {
                w.Write(displayCategory);
            }
            w.Write("] ");
        }

        private static void WriteExMarker(TextWriter w, bool useAnsi)
        {
            w.Write('[');
            if (useAnsi)
            {
                w.Write(AnsiRed);
                w.Write("Ex");
                w.Write(AnsiReset);
            }
            else
            {
                w.Write("Ex");
            }
            w.Write("] ");
        }

        private static void WriteExceptionLines(TextWriter w, string ts, LogLevel level, string displayCategory,
            Exception ex, bool useAnsi)
        {
            using var reader = new StringReader(ex.ToString());
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                WriteHeader(w, ts, level, displayCategory, useAnsi);
                WriteExMarker(w, useAnsi);
                w.Write(line);
                w.WriteLine();
            }
        }

        private static string ShortLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "INF"
        };

        private static string ShortCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return category;
            int lastDot = category.LastIndexOf('.');
            int lastPlus = category.LastIndexOf('+');
            int cut = Math.Max(lastDot, lastPlus);
            var name = cut >=0 ? category[(cut +1)..] : category;
            int tick = name.IndexOf('`');
            if (tick >=0) name = name[..tick];
            int lt = name.IndexOf('<');
            if (lt >=0) name = name[..lt];
            int bracket = name.IndexOf('[');
            if (bracket >=0) name = name[..bracket];
            return name;
        }

        private const string AnsiReset = "\u001b[0m";
        private const string AnsiCyan = "\u001b[36m";
        private const string AnsiRed = "\u001b[31m";
        private static string AnsiForLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => "\u001b[90m",
            LogLevel.Debug => "\u001b[37m",
            LogLevel.Information => "\u001b[32m",
            LogLevel.Warning => "\u001b[33m",
            LogLevel.Error => "\u001b[31m",
            LogLevel.Critical => "\u001b[97;41m",
            _ => ""
        };
    }
}
