// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Models
{
    /// <summary>
    /// Describes the current state of a PostgreSQL index.
    /// </summary>
    public class PostgresIndexStatus
    {
        /// <summary>
        /// Gets whether the index exists.
        /// </summary>
        public bool Exists { get; init; }

        /// <summary>
        /// Gets whether the index is valid and available for queries.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Gets the PostgreSQL index definition, or an empty string when the index does not exist.
        /// </summary>
        public string Definition { get; init; } = string.Empty;

        /// <summary>
        /// Gets the index size in bytes, or zero when the index does not exist.
        /// </summary>
        public long SizeBytes { get; init; }

        /// <summary>
        /// Gets whether an index is currently being built on the target table.
        /// </summary>
        public bool IsBuilding { get; init; }
    }
}
