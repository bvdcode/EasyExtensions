using System;
using System.IO;
using System.Collections.Generic;
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
            // [HH:mm:ss LVL] [Category] Message
            string ts = DateTime.Now.ToString("HH:mm:ss");
            var levelValue = logEntry.LogLevel;
            string lvl = ShortLevel(levelValue);
            string msg = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception) ?? string.Empty;
            string category = logEntry.Category ?? string.Empty;

            // Color policy from options
            var opts = _options.CurrentValue;
            bool colorsDisabled = opts.ColorBehavior == LoggerColorBehavior.Disabled;
            bool toConsole = !Console.IsOutputRedirected; // direct console writing allowed
            bool useAnsi = !colorsDisabled && !toConsole;

            if (toConsole && !colorsDisabled)
            {
                // Use Console.* APIs for reliable coloring when writing to a real console
                var originalFg = Console.ForegroundColor;
                var originalBg = Console.BackgroundColor;
                try
                {
                    Console.Write('[');
                    Console.Write(ts);
                    Console.Write(' ');
                    // level colored
                    switch (levelValue)
                    {
                        case LogLevel.Trace: Console.ForegroundColor = ConsoleColor.DarkGray; break;
                        case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Gray; break;
                        case LogLevel.Information: Console.ForegroundColor = ConsoleColor.Green; break;
                        case LogLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                        case LogLevel.Critical:
                            Console.BackgroundColor = ConsoleColor.Red; Console.ForegroundColor = ConsoleColor.White; break;
                    }
                    Console.Write(lvl);
                    Console.ForegroundColor = originalFg; Console.BackgroundColor = originalBg;
                    Console.Write("] ");

                    // category
                    Console.Write('[');
                    Console.Write(category);
                    Console.Write("] ");

                    // message
                    Console.Write(msg);

                    // structured state
                    if (logEntry.State is IEnumerable<KeyValuePair<string, object>> kvps)
                    {
                        bool first = true;
                        foreach (var kv in kvps)
                        {
                            if (kv.Key == "{OriginalFormat}") continue;
                            Console.Write(first ? " | " : ", ");
                            first = false;
                            Console.Write(kv.Key);
                            Console.Write('=');
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(kv.Value);
                            Console.ForegroundColor = originalFg;
                        }
                    }

                    if (logEntry.Exception is Exception ex)
                    {
                        Console.Write(" | ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(ex.GetType().Name);
                        Console.ForegroundColor = originalFg;
                        Console.Write(": ");
                        Console.Write(ex.Message);
                    }

                    Console.WriteLine();
                }
                finally
                {
                    Console.ForegroundColor = originalFg;
                    Console.BackgroundColor = originalBg;
                }
                return;
            }

            // Write header
            textWriter.Write('[');
            textWriter.Write(ts);
            textWriter.Write(' ');
            if (useAnsi)
            {
                textWriter.Write(AnsiForLevel(levelValue));
                textWriter.Write(lvl);
                textWriter.Write(AnsiReset);
            }
            else
            {
                textWriter.Write(lvl);
            }
            textWriter.Write("] ");

            // Category
            textWriter.Write('[');
            if (useAnsi)
            {
                textWriter.Write(AnsiCyan);
                textWriter.Write(category);
                textWriter.Write(AnsiReset);
            }
            else
            {
                textWriter.Write(category);
            }
            textWriter.Write("] ");

            // Message
            textWriter.Write(msg);

            // Structured state (highlight variables if present)
            if (logEntry.State is IEnumerable<KeyValuePair<string, object>> kvps2)
            {
                bool first = true;
                foreach (var kv in kvps2)
                {
                    if (kv.Key == "{OriginalFormat}") continue;
                    if (first)
                    {
                        textWriter.Write(" | ");
                        first = false;
                    }
                    else
                    {
                        textWriter.Write(", ");
                    }
                    textWriter.Write(kv.Key);
                    textWriter.Write('=');
                    if (useAnsi)
                    {
                        textWriter.Write(AnsiCyan);
                        textWriter.Write(kv.Value);
                        textWriter.Write(AnsiReset);
                    }
                    else
                    {
                        textWriter.Write(kv.Value);
                    }
                }
            }

            // Exception tail
            if (logEntry.Exception is Exception ex2)
            {
                textWriter.Write(" | ");
                if (useAnsi)
                {
                    textWriter.Write(AnsiRed);
                    textWriter.Write(ex2.GetType().Name);
                    textWriter.Write(AnsiReset);
                }
                else
                {
                    textWriter.Write(ex2.GetType().Name);
                }
                textWriter.Write(": ");
                textWriter.Write(ex2.Message);
            }

            textWriter.WriteLine();
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

        private const string AnsiReset = "\u001b[0m";
        private const string AnsiCyan = "\u001b[36m";
        private const string AnsiRed = "\u001b[31m";
        private static string AnsiForLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => "\u001b[90m",   // Bright black / DarkGray
            LogLevel.Debug => "\u001b[37m",   // Gray
            LogLevel.Information => "\u001b[32m", // Green
            LogLevel.Warning => "\u001b[33m", // Yellow
            LogLevel.Error => "\u001b[31m",   // Red
            LogLevel.Critical => "\u001b[97;41m", // White on Red background
            _ => ""
        };
    }
}
