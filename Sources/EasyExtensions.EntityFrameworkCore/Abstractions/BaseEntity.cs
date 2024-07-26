using System.ComponentModel.DataAnnotations.Schema;
using EasyExtensions.EntityFrameworkCore.Database.Repository;

namespace EasyExtensions.EntityFrameworkCore.Database.Abstractions
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
        /// Update entity method which called on <see cref="BaseRepository{TItem}.UpdateAsync(TItem)"/>.
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Update(BaseEntity entity);
    }
}