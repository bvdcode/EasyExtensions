using System;
using System.ComponentModel.DataAnnotations.Schema;
using EasyExtensions.EntityFrameworkCore.Database.Repository;

namespace EasyExtensions.EntityFrameworkCore.Database.Abstractions
{
    /// <summary>
    /// Auditable entity.
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
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
        /// Created by user identifier.
        /// </summary>
        [Column("created_by_user_id")]
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Updated by user identifier.
        /// </summary>
        [Column("updated_by_user_id")]
        public int UpdatedByUserId { get; set; }

        /// <summary>
        /// Implement this method to update entity.
        /// This method calling on <see cref="BaseRepository{TItem}.UpdateAsync(TItem)"/>.
        /// </summary>
        /// <param name="entity"> Updating entity. </param>
        protected abstract void Update(AuditableEntity entity);

        /// <summary>
        /// Update entity method which called on <see cref="BaseRepository{TItem}.UpdateAsync(TItem)"/>.
        /// </summary>
        /// <param name="entity"> Updating entity. </param>
        public override void Update(BaseEntity entity)
        {
            UpdatedAtUtc = DateTime.UtcNow;
            if (entity is AuditableEntity auditableEntity)
            {
                Update(auditableEntity);
                UpdatedByUserId = auditableEntity.UpdatedByUserId;
            }
        }
    }
}