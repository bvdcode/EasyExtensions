﻿using Quartz;
using System.Reflection;
using EasyExtensions.Helpers;
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
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
        {
            return services
                .AddQuartz(SetupQuartz)
                .AddQuartzHostedService();
        }

        private static void SetupQuartz(IServiceCollectionQuartzConfigurator configurator)
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
                });
            }
        }
    }
}