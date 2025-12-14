using EasyExtensions.EntityFrameworkCore.Abstractions;
using Gridify;
using System.Linq.Expressions;

namespace EasyExtensions.EntityFrameworkCore.Repository
{
    /// <summary>
    /// Represents a generic repository interface for accessing and manipulating entities of type TItem.
    /// </summary>
    /// <typeparam name="TItem">The type of entity.</typeparam>
    public interface IRepository<TItem> : IRepository where TItem : BaseEntity<int>
    {
        /// <summary>
        /// Retrieves an entity by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity with the specified ID.</returns>
        Task<TItem> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
        [Obsolete("Use AddAsync() instead.")]
        Task<TItem> CreateAsync(TItem item, CancellationToken cancellationToken = default);


        /// <summary>
        /// Creates a new entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
        Task<TItem> AddAsync(TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the deletion was successful.</returns>
        Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a range of entities asynchronously.
        /// </summary>
        /// <param name="items"> The entities to delete. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A task that represents the asynchronous operation. The task result contains the number of state entries written to the database. </returns>
        Task<int> DeleteRangeAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Filters and paginates the entities asynchronously based on the specified query.
        /// </summary>
        /// <param name="query">The query parameters for filtering and pagination.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        Task<Paging<TItem>> FilterAsync(IGridifyQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Filters and paginates the entities asynchronously based on the specified query and mapper.
        /// </summary>
        /// <param name="query">The query parameters for filtering and pagination.</param>
        /// <param name="mapper">The mapper used to map the entities.</param>
        /// <param name="listBeforeFiltering">Indicates whether to list the entities before filtering. Be careful with this option, as it may lead to performance issues if the dataset is large.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered and paginated entities.</returns>
        Task<Paging<TItem>> FilterAsync(IGridifyQuery query, IGridifyMapper<TItem> mapper, bool listBeforeFiltering = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        [Obsolete("Use ListAsync() instead.")]
        Task<IEnumerable<TItem>> FindAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities asynchronously based on the specified predicate and returns all found entities as <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the predicate.</returns>
        Task<IReadOnlyList<TItem>> ListAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation. The task result contains a collection of all entities. </returns>
        [Obsolete("Use ListAllAsync() instead.")]
        Task<IReadOnlyList<TItem>> ListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities asynchronously.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation. The task result contains a collection of all entities. </returns>
        Task<IReadOnlyList<TItem>> ListAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns an <see cref="IQueryable{T}"/> of entities.
        /// </summary>
        /// <returns> An <see cref="IQueryable{T}"/> of entities.</returns>
        [Obsolete("This methos will be removed in future versions.")]
        IQueryable<TItem> Query();

        /// <summary>
        /// Returns a count of entities that match the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities that match the predicate.</returns>
        Task<int> CountAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first entity that matches the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the predicate.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no entity is found that matches the predicate.</exception>
        Task<TItem> FirstAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first entity that matches the specified predicate asynchronously or null if no entity is found.
        /// </summary>
        /// <param name="predicate">The predicate used to filter the entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity that matches the predicate or null if no entity is found.</returns>
        Task<TItem?> FirstOrDefaultAsync(Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Base repository interface for accessing and manipulating entities.
    /// </summary>
    public interface IRepository { }
}