using Gridify;
using System.Reflection;
using EasyExtensions.Helpers;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Repository;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Sets up Gridify.
        /// </summary>
        public static IServiceCollection AddGridifyMappers(this IServiceCollection services)
        {
            GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
            services.AddGridifyMappers(Assembly.GetCallingAssembly());
            return services;
        }

        /// <summary>
        /// Adds all types that implement <see cref="IRepository"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var repositories = ReflectionHelpers.GetTypesOfInterface<IRepository>();
            foreach (var repository in repositories)
            {
                Type genericType = repository.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>));
                var descriptor = new ServiceDescriptor(genericType, repository, ServiceLifetime.Scoped);
                services.Add(descriptor);
            }
            return services;
        }
    }
}