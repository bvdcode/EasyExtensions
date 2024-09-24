using System;
using WebDav;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EasyExtensions.WebDav
{
    /// <summary>
    /// WebDAV cloud client.
    /// </summary>
    public class WebDavCloudClient : IWebDavCloudClient, IDisposable
    {
        private readonly string _baseAddress;
        private readonly WebDavClient _client;

        /// <summary>
        /// Creates a new instance of the <see cref="WebDavCloudClient"/> class.
        /// </summary>
        /// <param name="server"> The WebDAV server URL. </param>
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <param name="baseAddress"> The base address. </param>
        public WebDavCloudClient(string server, string username, string password, string? baseAddress = null)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            var parameters = new WebDavClientParams
            {
                BaseAddress = baseAddress == null ? new Uri(server) : ConcatUris(server, baseAddress),
                Credentials = new NetworkCredential(username, password),
            };
            _client = new WebDavClient(parameters);
            _baseAddress = parameters.BaseAddress.ToString();
        }

        private Uri ConcatUris(string server, string baseAddress)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(baseAddress))
            {
                throw new ArgumentException("Server and base address must not be null or empty.");
            }
            if (server.EndsWith('/') && baseAddress.StartsWith('/'))
            {
                return new Uri(server + baseAddress[1..]);
            }
            if (!server.EndsWith('/') && !baseAddress.StartsWith('/'))
            {
                return new Uri(server + '/' + baseAddress);
            }
            return new Uri(server + baseAddress);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WebDavCloudClient"/> class for Nextcloud Server.
        /// </summary>
        /// <param name="server"> The WebDAV server URL. </param>
        /// <param name="username"> The username. </param>
        /// <param name="password"> The password. </param>
        /// <returns> A new instance of the <see cref="WebDavCloudClient"/> class. </returns>
        public static WebDavCloudClient CreateNextcloudClient(string server, string username, string password)
        {
            string baseAddress = "/remote.php/dav/files/" + username + '/';
            return new WebDavCloudClient(server, username, password, baseAddress);
        }

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="bytes"> The file bytes. </param>
        /// <param name="filename"> The filename. </param>
        /// <returns> The HTTP status code. </returns>
        /// <exception cref="WebException"> When the file cannot be uploaded. </exception>
        public async Task UploadFileAsync(byte[] bytes, string filename)
        {
            var stream = new MemoryStream(bytes);
            await UploadFileAsync(stream, filename);
        }

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="fileStream"> The file stream. </param>
        /// <param name="filename"> The filename. </param>
        /// <returns> The HTTP status code. </returns>
        /// <exception cref="WebException"> When the file cannot be uploaded. </exception>
        public async Task UploadFileAsync(Stream fileStream, string filename)
        {
            string url = ConcatUris(_baseAddress, filename).ToString();
            var result = await _client.PutFile(url, fileStream);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                if (filename.Contains('/'))
                {
                    string folder = filename[..filename.LastIndexOf('/')];
                    if (!string.IsNullOrEmpty(folder))
                    {
                        await CreateFolderAsync(folder);
                        if (fileStream.CanSeek)
                        {
                            // Reset the stream position to the beginning (if possible)
                            fileStream.Seek(0, SeekOrigin.Begin);
                        }
                        result = await _client.PutFile(url, fileStream);
                    }
                }
            }
            if (result.StatusCode != (int)HttpStatusCode.Created
                && result.StatusCode != (int)HttpStatusCode.NoContent
                && result.StatusCode != (int)HttpStatusCode.MultiStatus)
            {
                throw new WebException($"Failed to upload file {url} with code {result.StatusCode}.");
            }
        }

        /// <summary>
        /// Checks if a file exists on the WebDAV server.
        /// </summary>
        /// <param name="filename"> The filename. </param>
        /// <returns> True if the file exists, false otherwise. </returns>
        public async Task<bool> ExistsAsync(string filename)
        {
            string url = ConcatUris(_baseAddress, filename).ToString();
            var result = await _client.Propfind(url);
            return result.StatusCode == (int)HttpStatusCode.MultiStatus;
        }

        /// <summary>
        /// Creates a folder on the WebDAV server if it does not exist.
        /// </summary>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The HTTP status code. </returns>
        /// <exception cref="WebException"> When the folder cannot be created. </exception>
        public async Task CreateFolderAsync(string folder)
        {
            string url = ConcatUris(_baseAddress, folder).ToString();
            var result = await _client.Mkcol(url);
            if (result.StatusCode == (int)HttpStatusCode.NotFound && folder.Contains('/'))
            {
                string parentFolder = folder[..folder.LastIndexOf('/')];
                if (!string.IsNullOrEmpty(parentFolder))
                {
                    await CreateFolderAsync(parentFolder);
                    result = await _client.Mkcol(url);
                }
            }
            if (result.StatusCode != (int)HttpStatusCode.Created
                && result.StatusCode != (int)HttpStatusCode.NoContent
                && result.StatusCode != (int)HttpStatusCode.MultiStatus)
            {
                throw new WebException($"Failed to create folder {url} with code {result.StatusCode}.");
            }
        }

        /// <summary>
        /// Disposes the <see cref="WebDavCloudClient"/> instance.
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
