using System;
using System.Linq;
using EasyExtensions.Helpers;
using EasyExtensions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="CpuUsageService"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddCpuUsageService(this IServiceCollection services)
        {
            services.AddSingleton(new CpuUsageService());
            return services;
        }

        /// <summary>
        /// Add all types inherited from IRepository.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            const string implementedInterfaceName = "IRepository";
            var types = ReflectionHelpers.GetTypesOfInterface(implementedInterfaceName);
            if (!types.Any())
            {
                throw new Exception($"No types inherited from {implementedInterfaceName} found.");
            }
            foreach (var type in types)
            {
                Type repo = type
                    .GetInterfaces()
                    .First(x => x.Name.Contains(implementedInterfaceName));
                var descriptor = new ServiceDescriptor(repo, type, ServiceLifetime.Scoped);
                services.Add(descriptor);
            }
            return services;
        }
    }
}