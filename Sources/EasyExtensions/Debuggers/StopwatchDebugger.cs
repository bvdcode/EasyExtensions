using System;
using System.Diagnostics;

namespace EasyExtensions.Debuggers
{
    /// <summary>
    /// StopwatchDebugger class to measure elapsed time.
    /// </summary>
    /// <remarks>
    /// Usage example:
    /// <code>
    /// var debugger = new StopwatchDebugger(x =>
    /// {
    ///    _logger.LogInformation($"{action} took {elapsed} seconds.", x.Action, x.Elapsed.TotalSeconds);
    /// });
    /// debugger.Report("Started");
    /// // Some code here
    /// debugger.Report("Finished");
    /// </code>
    /// </remarks>
    public class StopwatchDebugger
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<StopwatchDebuggerEventArgs> _callback;

        /// <summary>
        /// Create a new instance of StopwatchDebugger.
        /// </summary>
        /// <param name="callback">Callback function to report elapsed time.</param>
        public StopwatchDebugger(Action<StopwatchDebuggerEventArgs> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Report the elapsed time.
        /// </summary>
        /// <param name="action">Action name.</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null or empty.</exception>
        public void Report(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentNullException(nameof(action));
            }
            _stopwatch.Stop();
            _callback(new StopwatchDebuggerEventArgs
            {
                Action = action,
                Elapsed = _stopwatch.Elapsed
            });
            _stopwatch.Restart();
        }

        /// <summary>
        /// Restart the stopwatch.
        /// </summary>
        public void Restart()
        {
            _stopwatch.Restart();
        }

        /// <summary>
        /// StopwatchDebuggerEventArgs class to report elapsed time.
        /// </summary>
        public class StopwatchDebuggerEventArgs : EventArgs
        {
            /// <summary>
            /// Action name.
            /// </summary>
            public string Action { get; set; } = string.Empty;

            /// <summary>
            /// Elapsed time.
            /// </summary>
            public TimeSpan Elapsed { get; set; }
        }
    }
}
