using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using EasyExtensions.EntityFrameworkCore.Abstractions;

namespace EasyExtensions.EntityFrameworkCore.Database
{
    /// <summary>
    ///     A DbContext instance represents a session with the database and can be used to query and save
    ///     instances of your entities. DbContext is a combination of the Unit Of Work and Repository patterns.
    ///     This version of DbContext is designed for automated auditing of entities.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
    ///         and examples.
    ///     </para>
    ///     <para>
    ///         Typically you create a class that derives from DbContext and contains <see cref="DbSet{TEntity}" />
    ///         properties for each entity in the model. If the <see cref="DbSet{TEntity}" /> properties have a public setter,
    ///         they are automatically initialized when the instance of the derived context is created.
    ///     </para>
    ///     <para>
    ///         Override the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> method to configure the database (and
    ///         other options) to be used for the context. Alternatively, if you would rather perform configuration externally
    ///         instead of inline in your context, you can use <see cref="DbContextOptionsBuilder{TContext}" />
    ///         (or <see cref="DbContextOptionsBuilder" />) to externally create an instance of <see cref="DbContextOptions{TContext}" />
    ///         (or <see cref="DbContextOptions" />) and pass it to a base constructor of <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The model is discovered by running a set of conventions over the entity classes found in the
    ///         <see cref="DbSet{TEntity}" /> properties on the derived context. To further configure the model that
    ///         is discovered by convention, you can override the <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>,
    ///         <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>,
    ///         <see href="https://aka.ms/efcore-docs-change-tracking">Changing tracking</see>, and
    ///         <see href="https://aka.ms/efcore-docs-saving-data">Saving data with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>    
    public class AuditedDbContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext" /> class. The
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />
        ///     method will be called to configure the database (and other options) to be used for this context.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
        ///     for more information and examples.
        /// </remarks>
        protected AuditedDbContext() : base() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext" /> class using the specified options.
        ///     The <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> method will still be called to allow further
        ///     configuration of the options.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see> and
        ///     <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
        /// </remarks>
        /// <param name="options">The options for this context.</param>
        public AuditedDbContext(DbContextOptions options) : base(options) { }

        /// <summary>
        ///     Saves all changes made in this context to the database and update Modified and Created datetime.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" />
        ///         to discover any changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
        ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
        ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
        ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more information
        ///         and examples.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
        ///     </para>
        /// </remarks>
        /// <returns>
        ///     The number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        public override int SaveChanges()
        {
            UpdateDateTimeValues();
            return base.SaveChanges();
        }

        /// <summary>
        ///     Saves all changes made in this context to the database and update Modified and Created datetime.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" />
        ///         to discover any changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
        ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
        ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
        ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more
        ///         information and examples.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
        ///     </para>
        /// </remarks>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after
        ///     the changes have been sent successfully to the database.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateDateTimeValues();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        ///     Saves all changes made in this context to the database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="ChangeTracker.DetectChanges" />
        ///         to discover any changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
        ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
        ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
        ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see> for more
        ///         information and examples.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        /// <exception cref="DbUpdateException">
        ///     An error is encountered while saving to the database.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        ///     A concurrency violation is encountered while saving to the database.
        ///     A concurrency violation occurs when an unexpected number of rows are affected during save.
        ///     This is usually because the data in the database has been modified since it was loaded into memory.
        /// </exception>
        /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateDateTimeValues();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Autoset Modified and Created datetime.
        /// </summary>
        private void UpdateDateTimeValues()
        {
            var auditableEntries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity &&
                (e.State == EntityState.Modified || e.State == EntityState.Added));
            DateTime now = DateTime.UtcNow;
            foreach (var entityEntry in auditableEntries)
            {
                BaseEntity entity = (BaseEntity)entityEntry.Entity;
                entity.UpdatedAt = now;
                if (entityEntry.State == EntityState.Added && entity.CreatedAt == default)
                {
                    entity.CreatedAt = now;
                }
            }
        }
    }
}
