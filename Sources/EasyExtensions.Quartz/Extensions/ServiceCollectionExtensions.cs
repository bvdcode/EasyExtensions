using Quartz;
using System.Reflection;
using EasyExtensions.Helpers;
using Microsoft.Extensions.Logging;
using EasyExtensions.Quartz.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.Quartz.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Quartz jobs with <see cref="JobTriggerAttribute"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        /// <param name="logger">Logger instance if you want to see jobs registration in the log.</param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, ILogger? logger = null)
        {
            return services
                .AddQuartz(x => SetupQuartz(x, logger))
                .AddQuartzHostedService();
        }

        private static void SetupQuartz(IServiceCollectionQuartzConfigurator configurator, ILogger? logger)
        {
            var jobs = ReflectionHelpers.GetTypesOfInterface<IJob>();
            foreach (var job in jobs)
            {
                JobTriggerAttribute? jobTriggerAttribute = job.GetCustomAttribute<JobTriggerAttribute>();
                if (jobTriggerAttribute == null)
                {
                    continue;
                }
                var jobKey = new JobKey(job.Name);
                configurator.AddJob(job, jobKey, x => x
                    .WithIdentity(jobKey)
                    .DisallowConcurrentExecution());
                configurator.AddTrigger(opts =>
                {
                    if (jobTriggerAttribute.StartNow)
                    {
                        opts.StartNow();
                    }
                    opts.ForJob(jobKey)
                        .WithIdentity(job.Name + "Trigger")
                        .WithSimpleSchedule(x => x
                        .WithInterval(jobTriggerAttribute.Interval)
                        .RepeatForever());
                    logger?.LogInformation($"Job {job.Name} with trigger {job.Name + "Trigger"} added, interval: {jobTriggerAttribute.Interval}, job key: {jobKey.Name}");
                });
            }
        }
    }
}