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
        /// Start the job at the specified time.
        /// </summary>
        public TimeOfDay? StartAt { get; }

        /// <summary>
        /// Start the job immediately.
        /// </summary>
        public bool StartNow { get; }

        /// <summary>
        /// Repeat the job forever.
        /// </summary>
        public bool RepeatForever { get; }

        /// <summary>
        /// Create a new instance of <see cref="JobTriggerAttribute"/>.
        /// </summary>
        /// <param name="days"> Days. </param>
        /// <param name="hours"> Hours. </param>
        /// <param name="minutes"> Minutes. </param>
        /// <param name="seconds"> Seconds. </param>
        /// <param name="startNow"> Start now. </param>
        /// <param name="repeatForever"> Repeat forever. </param>
        /// <param name="startAt"> Start at. </param>
        public JobTriggerAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0,
            bool startNow = true, bool repeatForever = true, TimeOfDay? startAt = null)
        {
            if (days <= 0 && hours <= 0 && minutes <= 0 && seconds <= 0)
            {
                throw new ArgumentException("At least one of the parameters must be greater than 0.");
            }
            StartAt = startAt;
            StartNow = startNow;
            RepeatForever = repeatForever;
            Interval = new TimeSpan(days, hours, minutes, seconds);
        }
    }
}