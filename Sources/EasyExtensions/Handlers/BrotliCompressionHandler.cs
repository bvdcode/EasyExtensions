using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.IO.Compression;
using System.Threading.Tasks;

namespace EasyExtensions.Handlers
{
    internal class BrotliCompressionHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.Content.Headers.TryGetValues("Content-Encoding", out var ce) || ce.First() != "br")
            {
                return response;
            }
            var inputStream = await response.Content.ReadAsStreamAsync();
            using BrotliStream stream = new BrotliStream(inputStream, CompressionMode.Decompress);
            MemoryStream outputStream = new MemoryStream();
            await stream.CopyToAsync(outputStream);
            outputStream.Seek(default, SeekOrigin.Begin);
            var content = new StreamContent(outputStream);
            foreach (var item in response.Content.Headers)
            {
                if (!content.Headers.Contains(item.Key))
                {
                    content.Headers.Add(item.Key, item.Value);
                }
            }
            response.Content = content;
            return response;
        }
    }
}
