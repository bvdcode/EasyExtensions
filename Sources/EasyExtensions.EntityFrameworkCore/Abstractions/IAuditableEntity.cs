// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

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
