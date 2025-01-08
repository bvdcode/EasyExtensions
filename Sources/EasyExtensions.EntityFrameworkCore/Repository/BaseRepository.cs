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
    public abstract class BaseRepository<TItem>(DbSet<TItem> db, Func<CancellationToken, Task<int>>? saveChangesCallback)
        : IRepository<TItem> where TItem : BaseEntity
    {
        private readonly DbSet<TItem> db = db;
        private readonly Func<CancellationToken, Task<int>>? saveChangesCallback = saveChangesCallback;

        /// <summary>
        /// Create base repository based on database entity set without autosaving.
        /// </summary>
        /// <param name="db">Database set of entities.</param>
        public BaseRepository(DbSet<TItem> db) : this(db, null) { }

        /// <summary>
        /// Retrieves an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity with the specified ID.</returns>
        public virtual async Task<TItem> GetByIdAsync(int id)
        {
            var found = await db.FirstOrDefaultAsync(x => x.Id == id);
            return found ?? throw new EntityNotFoundException(typeof(TItem).Name + $"[id:{id}]");
        }

        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
        public virtual async Task<TItem> CreateAsync(TItem item)
        {
            var result = await db.AddAsync(item);
            await SaveChangesAsync();
            return result.Entity;
        }

        /// <summary>
        /// Deletes an entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the deletion was successful.</returns>
        public virtual async Task<bool> DeleteAsync(TItem item)
        {
            var found = await GetByIdAsync(item.Id);
            if (found != null)
            {
                if (item is IDeletableEntity deletableEntity)
                {
                    deletableEntity.Delete();
                }
                else
                {
                    db.Remove(found);
                }
                await SaveChangesAsync();
            }
            return found != null;
        }

        /// <summary>
        /// Deletes a range of entities asynchronously.
        /// </summary>
        /// <param name="items"> The entities to delete. </param>
        /// <returns> A task that represents the asynchronous operation. The task result contains the number of state entries written to the database. </returns>
        public virtual async Task<int> DeleteRangeAsync(IEnumerable<TItem> items)
        {
            int counter = 0;
            foreach (var item in items)
            {
                bool deleted = await DeleteAsync(item);
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
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        public async Task<TItem> UpdateAsync(TItem item)
        {
            var found = await GetByIdAsync(item.Id);
            found.Update(item);
            await SaveChangesAsync();
            return found;
        }

        /// <summary>
        /// Filters and paginates the entities asynchronously based on the specified query.
        /// </summary>
        /// <param name="query">The query parameters for filtering and pagination.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        public async Task<Paging<TItem>> FilterAsync(IGridifyQuery query)
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
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        public async Task<Paging<TItem>> FilterAsync(IGridifyQuery query, IGridifyMapper<TItem> mapper)
        {
            query ??= new GridifyQuery(1, 20, string.Empty, "id desc");
            if (string.IsNullOrWhiteSpace(query.OrderBy))
            {
                query.OrderBy = "id desc";
            }
            return await db.GridifyAsync(query, mapper);
        }

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        public virtual Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate)
        {
            var foundEntities = db.Where(predicate);
            return Task.FromResult(foundEntities.AsEnumerable());
        }

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate and returns all found entities as <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        public async Task<IList<TItem>> ListAsync(Expression<Func<TItem, bool>> predicate)
        {
            return await db.Where(predicate).ToListAsync();
        }
        
        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation. The task result contains a collection of all entities. </returns>
        public async Task<IList<TItem>> ListAsync()
        {
            return await db.ToListAsync();
        }

        /// <summary>
        /// Saves all changes made in the context to the underlying database asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync()
        {
            if (saveChangesCallback == null)
            {
                return 0;
            }
            return await saveChangesCallback.Invoke(CancellationToken.None);
        }

        /// <summary>
        /// Get queryable database set.
        /// </summary>
        /// <returns> Queryable database set. </returns>
        public IQueryable<TItem> Query()
        {
            return db;
        }
    }
}