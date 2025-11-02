using System;
using WebDav;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using EasyExtensions.Extensions;
using System.Collections.Generic;

namespace EasyExtensions.WebDav
{
    /// <summary>
    /// WebDAV cloud client.
    /// </summary>
    public class WebDavCloudClient : IWebDavCloudClient, IDisposable
    {
        private readonly string _server;
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
            _server = server;
        }

        private Uri ConcatUris(string part1, string part2)
        {
            if (string.IsNullOrEmpty(part1) || string.IsNullOrEmpty(part2))
            {
                throw new ArgumentException("Path parts must not be null or empty.");
            }
            if (part1.EndsWith('/') && part2.StartsWith('/'))
            {
                return new Uri(part1 + part2[1..]);
            }
            if (!part1.EndsWith('/') && !part2.StartsWith('/'))
            {
                return new Uri(part1 + '/' + part2);
            }
            return new Uri(part1 + part2);
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
        /// <param name="created"> The creation date. </param>
        /// <returns> The HTTP status code. </returns>
        /// <exception cref="WebException"> When the file cannot be uploaded. </exception>
        public async Task UploadFileAsync(byte[] bytes, string filename, DateTime? created = null)
        {
            using var stream = new MemoryStream(bytes);
            await UploadFileAsync(stream, filename, created);
        }

        /// <summary>
        /// Uploads a file to the WebDAV server.
        /// </summary>
        /// <param name="fileStream"> The file stream. </param>
        /// <param name="filename"> The filename. </param>
        /// <param name="created"> The creation date. </param>
        /// <returns> The HTTP status code. </returns>
        /// <exception cref="WebException"> When the file cannot be uploaded. </exception>
        public async Task UploadFileAsync(Stream fileStream, string filename, DateTime? created = null)
        {
            created ??= DateTime.UtcNow;
            if (created.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("The created date must be in UTC format.", nameof(created));
            }
            using MemoryStream memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = memoryStream.ToArray();
            string url = ConcatUris(_baseAddress, filename).ToString();
            PutFileParameters parameters = new PutFileParameters
            {
                Headers = new Dictionary<string, string>
                {
                    { "X-OC-CTime", created.Value.ToUnixTimestampSeconds().ToString() }
                }
            };
            var result = await _client.PutFile(url, memoryStream, parameters);
            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                if (filename.Contains('/'))
                {
                    string folder = filename[..filename.LastIndexOf('/')];
                    if (!string.IsNullOrEmpty(folder))
                    {
                        await CreateFolderAsync(folder);
                        using MemoryStream newMemoryStream = new MemoryStream(bytes);
                        result = await _client.PutFile(url, newMemoryStream);
                    }
                }
            }
            if (result.StatusCode != (int)HttpStatusCode.Created
                && result.StatusCode != (int)HttpStatusCode.NoContent
                && result.StatusCode != (int)HttpStatusCode.MultiStatus)
            {
                throw new WebException($"Failed to upload file {url} with code {result.StatusCode}.");
            }
            memoryStream.Dispose();
        }

        [Obsolete]
        private async Task<HttpStatusCode> ExistsWithStatusAsync(string filename)
        {
            string url = ConcatUris(_baseAddress, filename).ToString();
            var result = await _client.Propfind(url);
            return (HttpStatusCode)result.StatusCode;
        }

        /// <summary>
        /// Checks if a file or folder exists on the WebDAV server.
        /// </summary>
        /// <param name="filename"> The filename (or folder name). </param>
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
            var parts = folder.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string currentUrl = _baseAddress;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }
                if (currentUrl.EndsWith('/'))
                {
                    currentUrl = currentUrl[..^1];
                }
                currentUrl += '/' + part;
                await _client.Mkcol(currentUrl);
            }
            bool exists = await ExistsAsync(folder);
            if (!exists)
            {
                throw new WebException($"Failed to create folder {folder} - it does not exist after creation attempt.");
            }
        }

        private string[] GetPathVariations(string path)
        {
            string[] pathvariations = new string[]
            {
                path,
                GetServerAddress() + path,
                GetBaseAddress() + path,
                path.TrimEnd('/'),
                GetServerAddress() + path.TrimEnd('/'),
                GetBaseAddress() + path.TrimEnd('/'),
                path + '/',
                GetServerAddress() + path + '/',
                GetBaseAddress() + path + '/',
                '/' + path,
                GetServerAddress() + '/' + path,
                GetBaseAddress() + '/' + path,
                '/' + path + '/',
                GetServerAddress() + '/' + path + '/',
                GetBaseAddress() + '/' + path + '/',
                '/' + path.TrimEnd('/'),
                GetServerAddress() + '/' + path.TrimEnd('/'),
                GetBaseAddress() + '/' + path.TrimEnd('/'),
            };
            return pathvariations;
        }

        private bool IsEqual(string? path1, string? path2)
        {
            if (path1 == null || path2 == null)
            {
                return false;
            }

            var path1variations = GetPathVariations(path1);
            var path2variations = GetPathVariations(path2);

            return path1variations.Any(p1 => path2variations.Any(p2 => p1 == p2));
        }

        /// <summary>
        /// Lists all resources in a folder on the WebDAV server or gets the file if the path is a file.
        /// </summary>
        /// <param name="folder"> The folder or file path. </param>
        /// <returns> The list of resources - files and directories. </returns>
        public async Task<IEnumerable<WebDavResource>> GetResourcesAsync(string folder)
        {
            string url = ConcatUris(_baseAddress, folder).ToString();
            var result = await _client.Propfind(url);
            if (!result.IsSuccessful || result.StatusCode != (int)HttpStatusCode.MultiStatus)
            {
                return Array.Empty<WebDavResource>();
            }
            return result.Resources.Where(x =>
                !IsEqual(x.Uri, url) &&
                x.DisplayName != ".." &&
                x.DisplayName != ".");
        }

        /// <summary>
        /// Loads a file from the WebDAV server.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        /// <returns> The file bytes. </returns>
        public async Task<byte[]> GetFileBytesAsync(string filePath)
        {
            if (!await ExistsAsync(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }
            string requestUri = ConcatUris(_baseAddress, filePath).ToString();
            WebDavStreamResponse webDavStreamResponse = await _client.GetRawFile(requestUri);
            using MemoryStream memoryStream = new MemoryStream();
            await webDavStreamResponse.Stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Deletes a file or directory from the WebDAV server if it exists.
        /// </summary>
        /// <param name="filePath"> The file or directory path. </param>
        public async Task DeleteAsync(string filePath)
        {
            if (!await ExistsAsync(filePath))
            {
                return;
            }
            string url = ConcatUris(_baseAddress, filePath).ToString();
            var result = await _client.Delete(url);
            if (result.StatusCode != (int)HttpStatusCode.NoContent)
            {
                throw new WebException($"Failed to delete file {url} with code {result.StatusCode}.");
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="WebDavClient"/>.
        /// </summary>
        /// <returns> The <see cref="WebDavClient"/>. </returns>
        public WebDavClient GetWebDavClient()
        {
            return _client;
        }

        /// <summary>
        /// Gets the base address.
        /// </summary>
        /// <returns> The base address. </returns>
        public string GetBaseAddress()
        {
            return _baseAddress;
        }

        /// <summary>
        /// Gets the server URL.
        /// </summary>
        /// <returns> The server URL. </returns>
        public string GetServerAddress()
        {
            return _server;
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
