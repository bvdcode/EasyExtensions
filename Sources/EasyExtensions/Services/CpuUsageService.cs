using EasyExtensions.Models;
using System;
using System.Diagnostics;

namespace EasyExtensions.Services
{
    /// <summary>
    /// CPU usage service.
    /// </summary>
    public class CpuUsageService
    {
        private readonly Stopwatch stopwatch;

        /// <summary>
        /// Creates a new instance of <see cref="CpuUsageService"/> and starts the stopwatch.
        /// </summary>
        public CpuUsageService()
        {
            stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets the usage report.
        /// </summary>
        /// <returns> Usage report. </returns>
        public UsageReport GetUsage()
        {
            TimeSpan cpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            TimeSpan total = stopwatch.Elapsed;
            double usage = Math.Round(cpuUsage / total, 2);
            return new UsageReport()
            {
                Uptime = stopwatch.Elapsed,
                CpuUsage = usage,
            };
        }
    }
}