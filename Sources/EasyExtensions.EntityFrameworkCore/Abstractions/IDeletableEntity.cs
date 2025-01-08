using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Interface for deletable entity.
    /// </summary>
    public interface IDeletableEntity
    {
        /// <summary>
        /// When entity was deleted.
        /// </summary>
        [Column("deleted_at_utc")]
        public DateTime? DeletedAtUtc { get; set; }

        /// <summary>
        /// Calling this method must sets <see cref="DeletedAtUtc"/> to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        void Delete();
    }
}