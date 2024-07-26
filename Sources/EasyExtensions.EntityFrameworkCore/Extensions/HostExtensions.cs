using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IHost"/> extensions.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Applies migrations to the database.
        /// </summary>
        /// <returns> Current <see cref="IHost"/> instance. </returns>
        public static IHost ApplyMigrations<TContext>(this IHost host) where TContext : DbContext
        {
            using (var serviceScope = host.Services.GetService<IServiceScopeFactory>()!.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
                var migrations = context.Database.GetPendingMigrations();
                if (migrations.Any())
                {
                    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DbContext>>();
                    foreach (var migration in migrations)
                    {
                        logger.LogInformation("Applying migration {Migration}.", migration);
                    }
                    context.Database.Migrate();
                    logger.LogInformation("Migrations applied.");
                }
            }
            return host;
        }
    }
}