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
        /// Gets diagnostic SQL for display. Do not use it for compatibility comparisons.
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

        /// <summary>
        /// Gets the structured definition of a supported partial cosine HNSW index.
        /// Returns null for absent indexes or unsupported expressions and predicates.
        /// </summary>
        public PostgresVectorIndexDefinition? VectorDefinition { get; init; }

        /// <summary>
        /// Compares index structure with an expected definition independently of SQL formatting.
        /// Validity is checked separately using <see cref="IsValid"/>.
        /// </summary>
        /// <param name="expected">The same definition used to create the index.</param>
        /// <returns>Whether the index exists and its supported structure matches.</returns>
        public bool IsCompatibleWith(PostgresVectorIndexDefinition expected)
        {
            ArgumentNullException.ThrowIfNull(expected);
            return Exists && VectorDefinition == expected;
        }
    }
}
