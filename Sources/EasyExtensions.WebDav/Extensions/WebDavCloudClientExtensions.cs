using WebDav;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EasyExtensions.WebDav.Extensions
{
    /// <summary>
    /// <see cref="IWebDavCloudClient"/> extensions.
    /// </summary>
    public static class WebDavCloudClientExtensions
    {
        /// <summary>
        /// Lists the files in a folder on the WebDAV server.
        /// </summary>
        /// <param name="client"> The WebDAV cloud client. </param>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The list of files. </returns>
        public static async Task<IEnumerable<WebDavResource>> GetFilesAsync(this IWebDavCloudClient client, string folder)
        {
            var resources = await client.GetResourcesAsync(folder);
            return resources.Where(r => r.IsCollection == false);
        }

        /// <summary>
        /// Lists the directories in a folder on the WebDAV server.
        /// </summary>
        /// <param name="client"> The WebDAV cloud client. </param>
        /// <param name="folder"> The folder name. </param>
        /// <returns> The list of directories. </returns>
        public static async Task<IEnumerable<WebDavResource>> GetDirectoriesAsync(this IWebDavCloudClient client, string folder)
        {
            var resources = await client.GetResourcesAsync(folder);
            return resources.Where(r => r.IsCollection == true);
        }
    }
}
