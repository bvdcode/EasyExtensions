// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Models
{
    /// <summary>
    /// Defines a single-column cosine HNSW index with an integer equality predicate.
    /// </summary>
    /// <param name="SchemaName">The schema containing the table and index.</param>
    /// <param name="TableName">The unqualified table name.</param>
    /// <param name="IndexName">The unqualified index name.</param>
    /// <param name="VectorColumnName">The source vector column.</param>
    /// <param name="Dimensions">The vector dimensions.</param>
    /// <param name="FilterColumnName">The column used by the integer equality predicate.</param>
    /// <param name="FilterValue">The integer value required by the predicate.</param>
    public record PostgresVectorIndexDefinition(
        string SchemaName,
        string TableName,
        string IndexName,
        string VectorColumnName,
        int Dimensions,
        string FilterColumnName,
        int FilterValue);
}
