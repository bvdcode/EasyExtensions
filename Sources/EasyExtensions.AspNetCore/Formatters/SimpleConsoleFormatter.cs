using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyExtensions.AspNetCore.Formatters
{
    /// <summary>
    /// A simple console formatter that outputs log messages in a minimal format with optional color coding.
    /// </summary>
    public class SimpleConsoleFormatter : ConsoleFormatter
    {
        /// <summary>
        /// The name of the formatter.
        /// </summary>
        public const string FormatterName = nameof(SimpleConsoleFormatter);

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleConsoleFormatter"/> class with the name "minimal".
        /// </summary>
        public SimpleConsoleFormatter() : base(FormatterName) { }

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
            // [HH:mm:ss LVL] Message
            string ts = DateTime.Now.ToString("HH:mm:ss");
            var levelValue = logEntry.LogLevel;
            string lvl = ShortLevel(levelValue);
            string msg = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception) ?? string.Empty;

            bool canColor = !Console.IsOutputRedirected;
            var originalFg = canColor ? Console.ForegroundColor : default;
            var originalBg = canColor ? Console.BackgroundColor : default;

            void SetLevelColors(LogLevel level)
            {
                if (!canColor)
                {
                    return;
                }
                switch (level)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.DarkGray; break;
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray; break;
                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.Green; break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red; break;
                    case LogLevel.Critical:
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Red; break;
                    default:
                        Console.ForegroundColor = originalFg; break;
                }
            }

            void ResetColors()
            {
                if (!canColor) return;
                Console.ForegroundColor = originalFg;
                Console.BackgroundColor = originalBg;
            }

            // Write header
            textWriter.Write('[');
            textWriter.Write(ts);
            textWriter.Write(' ');
            SetLevelColors(levelValue);
            textWriter.Write(lvl);
            ResetColors();
            textWriter.Write("] ");

            // Message
            textWriter.Write(msg);

            // Structured state (highlight variables if present)
            if (logEntry.State is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                bool first = true;
                foreach (var kv in kvps)
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
                    if (canColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    textWriter.Write(kv.Value);
                    ResetColors();
                }
            }

            // Exception tail
            if (logEntry.Exception is Exception ex)
            {
                textWriter.Write(" | ");
                if (canColor)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                textWriter.Write(ex.GetType().Name);
                ResetColors();
                textWriter.Write(": ");
                textWriter.Write(ex.Message);
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
    }
}
