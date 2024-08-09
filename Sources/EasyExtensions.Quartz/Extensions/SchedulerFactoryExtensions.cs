using Quartz;
using System.Threading.Tasks;

namespace EasyExtensions.Quartz.Extensions
{
    /// <summary>
    /// <see cref="ISchedulerFactory"/> extensions.
    /// </summary>
    public static class SchedulerFactoryExtensions
    {
        /// <summary>
        /// Force triggers the job.
        /// </summary>
        /// <typeparam name="TJob">Job type.</typeparam>
        /// <param name="schedulerFactory"><see cref="ISchedulerFactory"/> instance.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public static async Task TriggerJobAsync<TJob>(this ISchedulerFactory schedulerFactory)
            where TJob : IJob
        {
            JobKey jobKey = new JobKey(typeof(TJob).Name);
            var scheduler = await schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(jobKey);
        }
    }
}
