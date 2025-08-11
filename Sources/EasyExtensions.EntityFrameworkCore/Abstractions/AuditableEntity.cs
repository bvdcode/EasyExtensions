using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Auditable entity with created and updated timestamps.
    /// </summary>
    public class AuditableEntity
    {
        /// <summary>
        /// Created at UTC.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; internal set; }

        /// <summary>
        /// Updated at UTC.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; internal set; }
    }
}