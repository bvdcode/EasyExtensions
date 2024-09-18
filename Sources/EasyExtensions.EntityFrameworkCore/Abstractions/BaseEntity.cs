using System.ComponentModel.DataAnnotations.Schema;
using EasyExtensions.EntityFrameworkCore.Repository;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Base entity.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Created at UTC.
        /// </summary>
        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Updated at UTC.
        /// </summary>
        [Column("updated_at_utc")]
        public DateTime UpdatedAtUtc { get; set; }

        /// <summary>
        /// Update entity method is calling in <see cref="BaseRepository{TItem}.UpdateAsync(TItem)"/>.
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