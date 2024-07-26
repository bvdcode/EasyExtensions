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
        /// Create a new instance of <see cref="JobTriggerAttribute"/>.
        /// </summary>
        /// <param name="days"> Days. </param>
        /// <param name="hours"> Hours. </param>
        /// <param name="minutes"> Minutes. </param>
        /// <param name="seconds"> Seconds. </param>
        public JobTriggerAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            if (days <= 0 && hours <= 0 && minutes <= 0 && seconds <= 0)
            {
                throw new ArgumentException("At least one of the parameters must be greater than 0.");
            }
            Interval = new TimeSpan(days, hours, minutes, seconds);
        }
    }
}