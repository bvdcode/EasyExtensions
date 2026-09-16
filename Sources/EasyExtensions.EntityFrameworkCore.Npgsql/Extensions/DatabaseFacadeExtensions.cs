// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

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
    }
}
