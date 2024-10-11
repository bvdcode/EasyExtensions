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
        /// <returns> The HTTP status code. </returns>
        Task UploadFileAsync(byte[] bytes, string filename);

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="fileStream"> The file stream. </param>
        /// <param name="filename"> The filename. </param>
        /// <returns> The HTTP status code. </returns>
        Task UploadFileAsync(Stream fileStream, string filename);

        /// <summary>
        /// Lists the files in a folder on the WebDAV server.
        /// </summary>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The list of files. </returns>
        Task<IEnumerable<WebDavResource>> GetFilesAsync(string folder);
    }
}
