using System;
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
        /// Update entity method which called on <see cref="BaseRepository{TItem}.UpdateAsync(TItem)"/>.
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Update(BaseEntity entity);
    }
}