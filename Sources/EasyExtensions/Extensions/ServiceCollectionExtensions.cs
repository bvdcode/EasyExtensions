using System;
using System.Linq;
using EasyExtensions.Helpers;
using EasyExtensions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.Extensions
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
        /// Add all types inherited from TInterface.
        /// </summary>
        /// <param name="services"> Current <see cref="IServiceCollection"/> instance. </param>
        /// <param name="serviceLifetime"> Service lifetime, default is Scoped. </param>
        /// <typeparam name="TInterface"> Interface type. </typeparam>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddTypesOfInterface<TInterface>(this IServiceCollection services, 
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TInterface : class
        {
            var types = ReflectionHelpers.GetTypesOfInterface<TInterface>();
            if (!types.Any())
            {
                throw new TypeLoadException($"No types inherited from {typeof(TInterface).Name} found.");
            }
            foreach (var type in types)
            {
                var descriptor = new ServiceDescriptor(typeof(TInterface), type, serviceLifetime);
                services.Add(descriptor);
            }
            return services;
        }
    }
}