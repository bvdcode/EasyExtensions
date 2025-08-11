namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Auditable entity contract with created and updated timestamps.
    /// </summary>
    internal interface IAuditableEntity
    {
        /// <summary>
        /// Created at UTC.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated at UTC.
        /// </summary>
        DateTime UpdatedAt { get; set; }
    }
}