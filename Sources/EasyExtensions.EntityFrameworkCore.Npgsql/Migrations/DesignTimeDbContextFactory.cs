using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Migrations
{
    /// <summary>
    /// This class is used to create a new instance of the <see cref="DbContext"/> with the specified arguments.
    /// Usually used in design time services, such as migrations.
    /// </summary>
    /// <typeparam name="TContext"> The type of <see cref="DbContext"/> to create. </typeparam>
    /// <param name="_serviceScopeFactory"> The <see cref="IServiceScopeFactory"/> instance. </param>
    public class DesignTimeDbContextFactory<TContext>(IServiceScopeFactory _serviceScopeFactory)
        : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DbContext"/> with the specified arguments.
        /// </summary>
        /// <param name="args"> The arguments to use when creating the <see cref="DbContext"/>. </param>
        /// <returns> A new instance of the <see cref="DbContext"/>. </returns>
        public TContext CreateDbContext(string[] args)
        {
            var serviceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            return serviceProvider.GetRequiredService<TContext>();
        }
    }
}
