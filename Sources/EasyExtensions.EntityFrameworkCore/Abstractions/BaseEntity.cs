// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Repository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyExtensions.EntityFrameworkCore.Abstractions
{
    /// <summary>
    /// Base generic entity.
    /// </summary>
    public abstract class BaseEntity<TId> : IAuditableEntity where TId : struct
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        [Key]
        [Column("id")]
        public TId Id { get; protected set; }

        /// <summary>
        /// Created at UTC.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Updated at UTC.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; private set; }

        DateTime IAuditableEntity.CreatedAt
        {
            get => CreatedAt;
            set => CreatedAt = value;
        }

        DateTime IAuditableEntity.UpdatedAt
        {
            get => UpdatedAt;
            set => UpdatedAt = value;
        }

        /// <summary>
        /// Update entity method is calling in <see cref="BaseDbSetRepository{TItem}.UpdateAsync(TItem, CancellationToken)"/>.
        /// Do not call this method from overriden method.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <exception cref="NotImplementedException">Update method is not overriden in inherited class.</exception>
        public virtual void Update(BaseEntity<TId> entity)
        {
            throw new NotImplementedException("Update method is not overriden in inherited class.");
        }
    }
}
