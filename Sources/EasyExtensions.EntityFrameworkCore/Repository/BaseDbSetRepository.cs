using Gridify;
using System.Linq.Expressions;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using EasyExtensions.EntityFrameworkCore.Exceptions;
using EasyExtensions.EntityFrameworkCore.Abstractions;

namespace EasyExtensions.EntityFrameworkCore.Repository
{
    /// <summary>
    /// Create base repository based on database entity set with autosaving.
    /// </summary>
    /// <param name="db">Database set of entities.</param>
    /// <param name="saveChangesCallback">Callback for changes saving.</param>
    public abstract class BaseDbSetRepository<TItem>(DbSet<TItem> db, Func<CancellationToken, Task<int>>? saveChangesCallback)
        : IRepository<TItem> where TItem : BaseEntity
    {
        private readonly DbSet<TItem> db = db;
        private readonly Func<CancellationToken, Task<int>>? saveChangesCallback = saveChangesCallback;

        /// <summary>
        /// Create base repository based on database entity set without autosaving.
        /// </summary>
        /// <param name="db">Database set of entities.</param>
        public BaseDbSetRepository(DbSet<TItem> db) : this(db, null) { }

        /// <summary>
        /// Retrieves an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity with the specified ID.</returns>
        public virtual async Task<TItem> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var found = await db.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
            return found ?? throw new EntityNotFoundException(typeof(TItem).Name + $"[id:{id}]");
        }

        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
        [Obsolete("Use AddAsync() instead.")]
        public virtual async Task<TItem> CreateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            var result = await db.AddAsync(item, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return result.Entity;
        }

        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
        public virtual async Task<TItem> AddAsync(TItem item, CancellationToken cancellationToken = default)
        {
            var result = await db.AddAsync(item, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return result.Entity;
        }

        /// <summary>
        /// Deletes an entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the deletion was successful.</returns>
        public virtual async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
        {
            var found = await GetByIdAsync(item.Id, cancellationToken);
            if (found != null)
            {
                if (found is IDeletableEntity deletableEntity)
                {
                    deletableEntity.Delete();
                }
                else
                {
                    db.Remove(found);
                }
                await SaveChangesAsync(cancellationToken);
            }
            return found != null;
        }

        /// <summary>
        /// Deletes a range of entities asynchronously.
        /// </summary>
        /// <param name="items"> The entities to delete. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A task that represents the asynchronous operation. The task result contains the number of state entries written to the database. </returns>
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            int counter = 0;
            foreach (var item in items)
            {
                bool deleted = await DeleteAsync(item, cancellationToken);
                if (deleted)
                {
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// Updates an existing entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        public virtual async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            var found = await GetByIdAsync(item.Id, cancellationToken);
            found.Update(item);
            await SaveChangesAsync(cancellationToken);
            return found;
        }

        /// <summary>
        /// Filters and paginates the entities asynchronously based on the specified query.
        /// </summary>
        /// <param name="query">The query parameters for filtering and pagination.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        public virtual async Task<Paging<TItem>> FilterAsync(IGridifyQuery query, CancellationToken cancellationToken = default)
        {
            query ??= new GridifyQuery(1, 20, string.Empty, "id desc");
            if (string.IsNullOrWhiteSpace(query.OrderBy))
            {
                query.OrderBy = "id desc";
            }
            return await db.GridifyAsync(query);
        }

        /// <summary>
        /// Filters and paginates the entities asynchronously based on the specified query and mapper.
        /// </summary>
        /// <param name="query">The query parameters for filtering and pagination.</param>
        /// <param name="mapper">The mapper used to map the entities.</param>
        /// <param name="listBeforeFiltering">Indicates whether to list the entities before filtering. Be careful with this option, as it may lead to performance issues if the dataset is large.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        public virtual async Task<Paging<TItem>> FilterAsync(IGridifyQuery query, IGridifyMapper<TItem> mapper, bool listBeforeFiltering = false, CancellationToken cancellationToken = default)
        {
            query ??= new GridifyQuery(1, 20, string.Empty, "id desc");
            if (string.IsNullOrWhiteSpace(query.OrderBy))
            {
                query.OrderBy = "id desc";
            }
            if (!listBeforeFiltering)
            {
                return await db.GridifyAsync(query, mapper);
            }
            var allItems = await ListAllAsync(cancellationToken);
            return allItems.AsQueryable().Gridify(query, mapper);
        }

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        public virtual Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var foundEntities = db.Where(predicate);
            return Task.FromResult(foundEntities.AsEnumerable());
        }

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate and returns all found entities as <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        public virtual async Task<IReadOnlyList<TItem>> ListAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await db.Where(predicate).ToListAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> A task that represents the asynchronous operation. The task result contains a collection of all entities. </returns>
        [Obsolete("Use ListAllAsync() instead.")]
        public virtual async Task<IReadOnlyList<TItem>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await db.ToListAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> A task that represents the asynchronous operation. The task result contains a collection of all entities. </returns>
        public virtual async Task<IReadOnlyList<TItem>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await db.ToListAsync(cancellationToken: cancellationToken);
        }

        private async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (saveChangesCallback == null)
            {
                return 0;
            }
            return await saveChangesCallback.Invoke(cancellationToken);
        }

        /// <summary>
        /// Get queryable database set.
        /// </summary>
        /// <returns> Queryable database set. </returns>
        [Obsolete("This method will be removed in future versions.")]
        public virtual IQueryable<TItem> Query()
        {
            return db;
        }

        /// <summary>
        /// Returns a count of entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities that match the predicate.</returns>
        public virtual Task<int> CountAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return db.CountAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Returns the first entity that matches the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the predicate.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no entity is found that matches the predicate.</exception>
        public virtual async Task<TItem> FirstAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await db.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken) 
                ?? throw new EntityNotFoundException($"No entity found that matches the predicate: {predicate}");
        }

        /// <summary>
        /// Returns the first entity that matches the specified predicate asynchronously or null if no entity is found.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the predicate or null if no entity is found.</returns>
        public virtual Task<TItem?> FirstOrDefaultAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return db.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        }
    }
}