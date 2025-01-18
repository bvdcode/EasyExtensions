using Quartz;
using System;
using System.Reflection;
using EasyExtensions.Helpers;
using EasyExtensions.Quartz.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Quartz.AspNetCore;

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
        /// <param name="jobAdded">Action to be executed when a job is added.</param>
        /// <param name="configureQuartz">Action to configure Quartz.</param>
        /// <param name="configureService">Action to configure Quartz hosted service.</param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, Action<Type>? jobAdded = null,
            Action<IServiceCollectionQuartzConfigurator>? configureQuartz = null, Action<QuartzHostedServiceOptions>? configureService = null)
        {
            return services
                .AddQuartz(x =>
                {
                    SetupQuartz(x, jobAdded);
                    configureQuartz?.Invoke(x);
                })
                .AddQuartzServer(x =>
                {
                    x.WaitForJobsToComplete = false;
                    x.AwaitApplicationStarted = true;
                    x.StartDelay = TimeSpan.FromSeconds(10);
                    configureService?.Invoke(x);
                });
        }

        private static void SetupQuartz(IServiceCollectionQuartzConfigurator configurator, Action<Type>? jobAdded)
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
                        .WithSimpleSchedule(x =>
                        {
                            x.WithInterval(jobTriggerAttribute.Interval);
                            if (jobTriggerAttribute.RepeatForever)
                            {
                                x.RepeatForever();
                            }
                        });
                    jobAdded?.Invoke(job);
                });
            }
        }
    }
}