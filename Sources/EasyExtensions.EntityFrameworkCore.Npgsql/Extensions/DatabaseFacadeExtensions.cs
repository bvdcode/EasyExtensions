// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Npgsql.Models;
using EasyExtensions.EntityFrameworkCore.Npgsql.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Extensions
{
    /// <summary>
    /// Provides PostgreSQL-specific extension methods for database metadata queries.
    /// </summary>
    public static class DatabaseFacadeExtensions
    {
        private const string InstalledExtensionQuery =
            "SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_extension WHERE extname = {0}) AS \"Value\"";
        private const string AvailableExtensionQuery =
            "SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_available_extensions WHERE name = {0}) AS \"Value\"";

        /// <summary>
        /// Determines whether a PostgreSQL extension is installed in the current database.
        /// </summary>
        /// <param name="database">The database facade used to execute the metadata query.</param>
        /// <param name="extensionName">The PostgreSQL extension name.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> when the extension is installed; otherwise, <see langword="false"/>.</returns>
        public static Task<bool> IsExtensionInstalledAsync(
            this DatabaseFacade database,
            string extensionName,
            CancellationToken cancellationToken = default)
        {
            return ExtensionExistsAsync(
                database,
                InstalledExtensionQuery,
                extensionName,
                cancellationToken);
        }

        /// <summary>
        /// Determines whether a PostgreSQL extension is available for installation on the server.
        /// </summary>
        /// <param name="database">The database facade used to execute the metadata query.</param>
        /// <param name="extensionName">The PostgreSQL extension name.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns><see langword="true"/> when the extension is available; otherwise, <see langword="false"/>.</returns>
        public static Task<bool> IsExtensionAvailableAsync(
            this DatabaseFacade database,
            string extensionName,
            CancellationToken cancellationToken = default)
        {
            return ExtensionExistsAsync(
                database,
                AvailableExtensionQuery,
                extensionName,
                cancellationToken);
        }

        /// <summary>
        /// Creates a partial PostgreSQL HNSW index using cosine distance without blocking writes to the table.
        /// </summary>
        /// <param name="database">The database facade used to create the index.</param>
        /// <param name="schemaName">The unqualified schema name containing the table.</param>
        /// <param name="tableName">The unqualified table name.</param>
        /// <param name="indexName">The unqualified index name.</param>
        /// <param name="vectorColumnName">The vector column indexed by HNSW.</param>
        /// <param name="dimensions">The vector dimensions used by the index expression.</param>
        /// <param name="filterColumnName">The integer column used by the partial-index predicate.</param>
        /// <param name="filterValue">The value required by the partial-index predicate.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">A transaction is active on the database facade.</exception>
        /// <remarks>PostgreSQL does not allow <c>CREATE INDEX CONCURRENTLY</c> inside a transaction.</remarks>
        public static async Task CreateVectorCosineHnswIndexConcurrentlyAsync(
            this DatabaseFacade database,
            string schemaName,
            string tableName,
            string indexName,
            string vectorColumnName,
            int dimensions,
            string filterColumnName,
            int filterValue,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(database);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);

            if (database.CurrentTransaction is not null)
            {
                throw new InvalidOperationException(
                    "CREATE INDEX CONCURRENTLY cannot run inside a transaction.");
            }

            string quotedTableName = QualifyIdentifier(schemaName, tableName, nameof(tableName));
            string quotedIndexName = QuoteIdentifier(indexName, nameof(indexName));
            string quotedVectorColumnName = QuoteIdentifier(vectorColumnName, nameof(vectorColumnName));
            string quotedFilterColumnName = QuoteIdentifier(filterColumnName, nameof(filterColumnName));
            string query = FormattableString.Invariant($"""
                CREATE INDEX CONCURRENTLY IF NOT EXISTS {quotedIndexName}
                ON {quotedTableName}
                USING hnsw (({quotedVectorColumnName}::vector({dimensions})) vector_cosine_ops)
                WHERE {quotedFilterColumnName} = {filterValue}
                """);

            await database.ExecuteSqlRawAsync(query, cancellationToken);
        }

        /// <summary>
        /// Creates a partial cosine HNSW index from a reusable structured definition.
        /// </summary>
        /// <param name="database">The database facade used to create the index.</param>
        /// <param name="definition">The definition also used for compatibility checks.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing index creation.</returns>
        public static Task CreateVectorCosineHnswIndexConcurrentlyAsync(
            this DatabaseFacade database,
            PostgresVectorIndexDefinition definition,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(definition);
            return database.CreateVectorCosineHnswIndexConcurrentlyAsync(
                definition.SchemaName,
                definition.TableName,
                definition.IndexName,
                definition.VectorColumnName,
                definition.Dimensions,
                definition.FilterColumnName,
                definition.FilterValue,
                cancellationToken);
        }

        /// <summary>
        /// Gets index status and structured metadata for a supported partial cosine HNSW index.
        /// </summary>
        /// <param name="database">The database facade used to query index metadata.</param>
        /// <param name="schemaName">The unqualified schema name containing the table and index.</param>
        /// <param name="tableName">The unqualified table name.</param>
        /// <param name="indexName">The unqualified index name.</param>
        /// <param name="cancellationToken">The token used to cancel the asynchronous operation.</param>
        /// <returns>The current index status.</returns>
        public static async Task<PostgresIndexStatus> GetIndexStatusAsync(
            this DatabaseFacade database,
            string schemaName,
            string tableName,
            string indexName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(database);

            string qualifiedIndexName = QualifyIdentifier(schemaName, indexName, nameof(indexName));
            string qualifiedTableName = QualifyIdentifier(schemaName, tableName, nameof(tableName));

            return await PostgresIndexMetadataReader.ReadAsync(
                database, qualifiedIndexName, qualifiedTableName, cancellationToken);
        }

        private static async Task<bool> ExtensionExistsAsync(
            DatabaseFacade database,
            string query,
            string extensionName,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(database);
            ArgumentException.ThrowIfNullOrWhiteSpace(extensionName);

            string normalizedExtensionName = extensionName.Trim();

            return await database
                .SqlQueryRaw<bool>(query, normalizedExtensionName)
                .SingleAsync(cancellationToken);
        }

        private static string QualifyIdentifier(
            string schemaName,
            string identifier,
            string parameterName)
        {
            return $"{QuoteIdentifier(schemaName, nameof(schemaName))}.{QuoteIdentifier(identifier, parameterName)}";
        }

        private static string QuoteIdentifier(string identifier, string parameterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(identifier, parameterName);

            if (identifier.Contains('\0'))
            {
                throw new ArgumentException("PostgreSQL identifiers cannot contain null characters.", parameterName);
            }

            return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }
    }
}
