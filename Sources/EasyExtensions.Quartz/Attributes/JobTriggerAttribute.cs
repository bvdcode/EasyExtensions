// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Quartz;
using System;

namespace EasyExtensions.Quartz.Attributes
{
    /// <summary>
    /// Add this attribute to the class inherited from <see cref="IJob"/> to set the trigger interval.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JobTriggerAttribute : Attribute
    {
        /// <summary>
        /// Job trigger interval.
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        /// Start the job immediately.
        /// </summary>
        public bool StartNow { get; }

        /// <summary>
        /// Repeat the job forever.
        /// </summary>
        public bool RepeatForever { get; }

        /// <summary>
        /// Cron schedule format. If specified, it will override the interval.
        /// </summary>
        public string? CronSchedule { get; }

        /// <summary>
        /// Gets or sets a value indicating whether concurrent execution of this operation is disallowed.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, attempts to execute the operation concurrently
        /// may be prevented, depending on the implementation. This can be used to ensure that only one instance of the
        /// operation runs at a time.</remarks>
        public bool DisallowConcurrentExecution { get; set; } = true;

        /// <summary>
        /// Create a new instance of <see cref="JobTriggerAttribute"/>.
        /// </summary>
        /// <param name="days"> Interval: Days. </param>
        /// <param name="hours"> Interval: Hours. </param>
        /// <param name="minutes"> Interval: Minutes. </param>
        /// <param name="seconds"> Interval: Seconds. </param>
        /// <param name="startNow"> Start now. </param>
        /// <param name="repeatForever"> Interval: Repeat forever. </param>
        /// <param name="disallowConcurrentExecution"> Indicates whether concurrent execution of this operation is disallowed. </param>
        /// <param name="cronSchedule"> Cron schedule, if specified, it will override the interval. <br/>
        /// See <see href="https://www.quartz-scheduler.org/documentation/quartz-2.3.0/tutorials/crontrigger.html">Quartz CronTrigger Documentation</see> for more information. </param>
        public JobTriggerAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0,
            bool startNow = true, bool repeatForever = true, string? cronSchedule = "", bool disallowConcurrentExecution = true)
        {
            TimeSpan interval = new TimeSpan(days, hours, minutes, seconds);
            if (interval <= TimeSpan.Zero && string.IsNullOrWhiteSpace(cronSchedule))
            {
                throw new ArgumentException("At least one of the parameters must be greater than 0 or CronSchedule must be specified.");
            }
            StartNow = startNow;
            CronSchedule = cronSchedule;
            RepeatForever = repeatForever;
            Interval = new TimeSpan(days, hours, minutes, seconds);
            DisallowConcurrentExecution = disallowConcurrentExecution;
        }
    }
}
