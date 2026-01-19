// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebDav;

namespace EasyExtensions.WebDav
{
    /// <summary>
    /// Interface for the WebDAV cloud client.
    /// </summary>
    public interface IWebDavCloudClient
    {
        /// <summary>
        /// Gets the underlying <see cref="WebDavClient"/>.
        /// </summary>
        /// <returns> The <see cref="WebDavClient"/>. </returns>
        WebDavClient GetWebDavClient();

        /// <summary>
        /// Gets the base address.
        /// </summary>
        /// <returns> The base address. </returns>
        string GetBaseAddress();

        /// <summary>
        /// Gets the server URL.
        /// </summary>
        /// <returns> The server. </returns>
        string GetServerAddress();

        /// <summary>
        /// Creates a folder on the WebDAV server if it does not exist.
        /// </summary>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The HTTP status code. </returns>
        Task CreateFolderAsync(string folder);

        /// <summary>
        /// Checks if a file exists on the WebDAV server.
        /// </summary>
        /// <param name="filename"> The filename. </param>
        /// <returns> True if the file exists, false otherwise. </returns>
        Task<bool> ExistsAsync(string filename);

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="bytes"> The file bytes. </param>
        /// <param name="filename"> The filename. </param>
        /// <param name="created"> The created date. </param>
        /// <returns> The HTTP status code. </returns>
        Task UploadFileAsync(byte[] bytes, string filename, DateTime? created = null);

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="fileStream"> The file stream. </param>
        /// <param name="filename"> The filename. </param>
        /// <param name="created"> The created date. </param>
        /// <returns> The HTTP status code. </returns>
        Task UploadFileAsync(Stream fileStream, string filename, DateTime? created = null);

        /// <summary>
        /// Lists all resources in a folder on the WebDAV server.
        /// </summary>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The list of resources - files and directories. </returns>
        Task<IEnumerable<WebDavResource>> GetResourcesAsync(string folder);

        /// <summary>
        /// Loads a file from the WebDAV server.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        /// <returns> The file bytes. </returns>
        Task<byte[]> GetFileBytesAsync(string filePath);

        /// <summary>
        /// Deletes a file or directory from the WebDAV server if it exists.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        Task DeleteAsync(string filePath);
    }
}
