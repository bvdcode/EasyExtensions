using Quartz;
using System;
using Quartz.AspNetCore;
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
        /// Adds Quartz jobs with <see cref="JobTriggerAttribute"/> to the <see cref="IServiceCollection"/>
        /// and automatically configures Quartz and Quartz hosted service with default options.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        /// <param name="jobAdded">Action to be executed when a job is added.</param>
        /// <param name="configureQuartz">Action to configure Quartz.</param>
        /// <param name="configureService">Action to configure Quartz hosted service.</param>
        /// <param name="postgresConnectionString">Postgres connection string for persistent store. If null or empty, in-memory store is used.</param>
        /// <param name="useClustering">Whether to use clustering with the persistent store. Default is false.</param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, Action<Type>? jobAdded = null,
            Action<IServiceCollectionQuartzConfigurator>? configureQuartz = null, Action<QuartzHostedServiceOptions>? configureService = null,
            string? postgresConnectionString = null, bool useClustering = false)
        {
            return services.AddQuartz(x =>
            {
                SetupQuartz(x, jobAdded);
                configureQuartz?.Invoke(x);
                if (!string.IsNullOrWhiteSpace(postgresConnectionString))
                {
                    x.UsePersistentStore(ps =>
                    {
                        ps.UseProperties = true;
                        ps.UsePostgres(postgresConnectionString);
                        if (useClustering)
                        {
                            ps.UseClustering();
                        }
                    });
                }
            })
            .AddQuartzServer(x =>
            {
                x.WaitForJobsToComplete = false;
                x.AwaitApplicationStarted = true;
                x.StartDelay = TimeSpan.FromSeconds(5);
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
                    .DisallowConcurrentExecution(concurrentExecutionDisallowed: jobTriggerAttribute.DisallowConcurrentExecution));
                configurator.AddTrigger(opts =>
                {
                    opts.ForJob(jobKey)
                        .WithIdentity(job.Name + "Trigger");
                    if (jobTriggerAttribute.Interval > TimeSpan.Zero && string.IsNullOrWhiteSpace(jobTriggerAttribute.CronSchedule))
                    {
                        opts.WithSimpleSchedule(x =>
                        {
                            x.WithInterval(jobTriggerAttribute.Interval)
                                .WithMisfireHandlingInstructionFireNow();
                            if (jobTriggerAttribute.RepeatForever)
                            {
                                x.RepeatForever();
                            }
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(jobTriggerAttribute.CronSchedule))
                    {
                        opts.WithCronSchedule(jobTriggerAttribute.CronSchedule);
                    }
                    else
                    {
                        throw new ArgumentException("At least one of the parameters must be specified - Interval or CronSchedule.");
                    }
                    if (jobTriggerAttribute.StartNow)
                    {
                        opts.StartNow();
                    }
                    jobAdded?.Invoke(job);
                });
            }
        }
    }
}