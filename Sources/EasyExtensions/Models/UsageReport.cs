using System;

namespace EasyExtensions.Models
{
    /// <summary>
    /// Usage report.
    /// </summary>
    public class UsageReport
    {
        /// <summary>
        /// Server uptime.
        /// </summary>
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// CPU usage from 0 to 1.
        /// </summary>
        public double CpuUsage { get; set; }
    }
}