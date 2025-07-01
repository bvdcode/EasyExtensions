using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyExtensions.EntityFrameworkCore.Repository;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Base entity with <see cref="int"/> identifier.
    /// </summary>
    public abstract class BaseEntity : BaseEntity<int>
    {

    }

    /// <summary>
    /// Base generic entity.
    /// </summary>
    public abstract class BaseEntity<TId> : AuditableEntity where TId : struct
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        [Key]
        [Column("id")]
        public TId Id { get; protected set; }

        /// <summary>
        /// Update entity method is calling in <see cref="BaseDbSetRepository{TItem}.UpdateAsync(TItem, CancellationToken)"/>.
        /// Do not call this method from overriden method.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <exception cref="NotImplementedException">Update method is not overriden in inherited class.</exception>
        public virtual void Update(BaseEntity entity)
        {
            throw new NotImplementedException("Update method is not overriden in inherited class.");
        }
    }
}
