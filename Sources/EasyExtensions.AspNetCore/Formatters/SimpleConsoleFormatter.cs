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
            string displayCategory = ShortCategory(category);

            // Color policy from options
            var opts = _options.CurrentValue;
            bool colorsDisabled = opts.ColorBehavior == LoggerColorBehavior.Disabled;
            bool toConsole = !Console.IsOutputRedirected; // detect console, but we still write via textWriter
            bool useAnsi = !colorsDisabled && !toConsole; // keep prior behavior to avoid escape codes on real console

            // Local helper to write the common header: [ts LVL] [Category]
            void WriteHeader()
            {
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

                textWriter.Write('[');
                if (useAnsi)
                {
                    textWriter.Write(AnsiCyan);
                    textWriter.Write(displayCategory);
                    textWriter.Write(AnsiReset);
                }
                else
                {
                    textWriter.Write(displayCategory);
                }
                textWriter.Write("] ");
            }

            // Local helper to write the [Ex] marker
            void WriteExMarker()
            {
                textWriter.Write('[');
                if (useAnsi)
                {
                    textWriter.Write(AnsiRed);
                    textWriter.Write("Ex");
                    textWriter.Write(AnsiReset);
                }
                else
                {
                    textWriter.Write("Ex");
                }
                textWriter.Write("] ");
            }

            // First line: header + message
            WriteHeader();
            textWriter.Write(msg);

            // For non-exception logs, render structured state inline on the same line (original behavior)
            if (logEntry.Exception is null && logEntry.State is IEnumerable<KeyValuePair<string, object>> inlineKvps)
            {
                bool first = true;
                foreach (var kv in inlineKvps)
                {
                    if (kv.Key == "{OriginalFormat}")
                    {
                        continue;
                    }
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

            textWriter.WriteLine();

            // If there's an exception, emit extra lines with [Ex] containing structured state and full exception details
            if (logEntry.Exception is Exception ex)
            {
                // Structured state (variables) printed on a separate [Ex] line when there is an exception
                if (logEntry.State is IEnumerable<KeyValuePair<string, object>> kvps)
                {
                    bool any = false;
                    foreach (var kv in kvps)
                    {
                        if (kv.Key == "{OriginalFormat}")
                            continue;
                        any = true; break;
                    }
                    if (any)
                    {
                        WriteHeader();
                        WriteExMarker();
                        textWriter.Write("- ");

                        bool first = true;
                        foreach (var kv in kvps)
                        {
                            if (kv.Key == "{OriginalFormat}")
                            {
                                continue;
                            }
                            if (!first)
                            {
                                textWriter.Write(", ");
                            }
                            first = false;

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
                        textWriter.WriteLine();
                    }
                }

                // Exception details: print the full exception ToString() (includes message, stack, inner exceptions)
                string exText = ex.ToString();
                using (var reader = new StringReader(exText))
                {
                    string? line;
                    while ((line = reader.ReadLine()) is not null)
                    {
                        WriteHeader();
                        WriteExMarker();
                        textWriter.Write(line);
                        textWriter.WriteLine();
                    }
                }
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
            // Trim namespace and nested type separators
            int lastDot = category.LastIndexOf('.');
            int lastPlus = category.LastIndexOf('+');
            int cut = Math.Max(lastDot, lastPlus);
            var name = cut >= 0 ? category[(cut + 1)..] : category;
            // Trim generic arity/backticks and generic argument list markers if any
            int tick = name.IndexOf('`');
            if (tick >= 0) name = name[..tick];
            int lt = name.IndexOf('<');
            if (lt >= 0) name = name[..lt];
            int bracket = name.IndexOf('[');
            if (bracket >= 0) name = name[..bracket];
            return name;
        }

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
