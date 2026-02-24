// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

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
    }
}
