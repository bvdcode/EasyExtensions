using Microsoft.AspNetCore.Http;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="HttpRequest"/> extensions.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Get remote host IP address using proxy "X-Real-IP", "CF-Connecting-IP", "X-Forwarded-For" headers, or connection remote IP address.
        /// </summary>
        /// <returns> IP address, or "Unknown" by default. </returns>
        public static string GetRemoteAddress(this HttpRequest request)
        {
            const string defaultResponce = "Unknown";
            string[] addressHeaders = new string[] { "CF-Connecting-IP", "X-Real-IP", "X-Forwarded-For" };
            if (request == null || request.HttpContext == null || request.HttpContext.Request == null)
            {
                return defaultResponce;
            }

            foreach (var addressHeader in addressHeaders)
            {
                if (request.HttpContext.Request.Headers.TryGetValue(addressHeader, out var value))
                {
                    return value!;
                }
            }

            if (request.HttpContext.Connection?.RemoteIpAddress != null)
            {
                return request.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::ffff:", string.Empty);
            }

            return defaultResponce;
        }
    }
}