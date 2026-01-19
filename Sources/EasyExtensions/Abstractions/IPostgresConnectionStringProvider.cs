// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Abstractions
{
    /// <summary>
    /// Defines a mechanism for retrieving a PostgreSQL database connection string.
    /// </summary>
    /// <remarks>Implementations of this interface can provide connection strings from various sources, such
    /// as configuration files, environment variables, or secure vaults. This abstraction enables flexible management of
    /// database connection details across different environments.</remarks>
    public interface IPostgresConnectionStringProvider
    {
        /// <summary>
        /// Retrieves the connection string used to establish a connection to the data source.
        /// </summary>
        /// <returns>A string containing the connection details required to connect to the data source. The format and contents
        /// of the string depend on the specific data provider.</returns>
        string GetConnectionString();
    }
}
