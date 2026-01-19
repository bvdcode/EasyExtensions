// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

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
