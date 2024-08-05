using Gridify;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
    }
}