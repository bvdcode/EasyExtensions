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
        /// Create a new instance of <see cref="JobTriggerAttribute"/>.
        /// </summary>
        /// <param name="days"> Interval: Days. </param>
        /// <param name="hours"> Interval: Hours. </param>
        /// <param name="minutes"> Interval: Minutes. </param>
        /// <param name="seconds"> Interval: Seconds. </param>
        /// <param name="startNow"> Start now. </param>
        /// <param name="repeatForever"> Interval: Repeat forever. </param>
        /// <param name="cronSchedule"> Cron schedule, if specified, it will override the interval. </param>
        public JobTriggerAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0,
            bool startNow = true, bool repeatForever = true, string? cronSchedule = "")
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
        }
    }
}